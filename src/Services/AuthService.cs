using Api.Data;
using Api.DTOs.Auth;
using Api.Models;
using Api.Services.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Api.Services;

public class AuthService : IAuthService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ITokenService _tokenService;
    private readonly AppDbContext _db;
    private readonly IConfiguration _config;

    public AuthService(
        UserManager<ApplicationUser> userManager,
        ITokenService tokenService,
        AppDbContext db,
        IConfiguration config)
    {
        _userManager = userManager;
        _tokenService = tokenService;
        _db = db;
        _config = config;
    }

    public async Task<AuthResponseDto> RegisterAsync(RegisterRequestDto dto)
    {
        var existing = await _userManager.FindByEmailAsync(dto.Email);
        if (existing is not null)
            throw new InvalidOperationException("Email is already in use.");

        var user = new ApplicationUser
        {
            Email = dto.Email,
            UserName = dto.Email,
            DisplayName = dto.DisplayName,
        };

        var result = await _userManager.CreateAsync(user, dto.Password);
        if (!result.Succeeded)
            throw new InvalidOperationException(string.Join(", ", result.Errors.Select(e => e.Description)));

        await _userManager.AddToRoleAsync(user, "User");
        var roles = await _userManager.GetRolesAsync(user);
        var token = _tokenService.GenerateAccessToken(user, roles);

        return new AuthResponseDto
        {
            AccessToken = token,
            ExpiresIn = _config.GetValue<int>("Jwt:ExpiresInMinutes", 15) * 60,
            User = MapUserDto(user, roles),
        };
    }

    public async Task<(AuthResponseDto Auth, string RefreshToken)> LoginAsync(LoginRequestDto dto)
    {
        var user = await _userManager.FindByEmailAsync(dto.Email);
        // Always check password even if user not found — prevents timing attacks
        if (user is null || !await _userManager.CheckPasswordAsync(user, dto.Password))
            throw new UnauthorizedAccessException("Invalid email or password.");

        var roles = await _userManager.GetRolesAsync(user);
        var accessToken = _tokenService.GenerateAccessToken(user, roles);
        var refreshToken = _tokenService.GenerateRefreshToken();

        user.RefreshToken = refreshToken;
        user.RefreshTokenExpiry = DateTime.UtcNow.AddDays(_config.GetValue<int>("Jwt:RefreshExpiryDays", 7));
        await _userManager.UpdateAsync(user);

        return (new AuthResponseDto
        {
            AccessToken = accessToken,
            ExpiresIn = _config.GetValue<int>("Jwt:ExpiresInMinutes", 15) * 60,
            User = MapUserDto(user, roles),
        }, refreshToken);
    }

    public async Task<(AuthResponseDto Auth, string RefreshToken)> RefreshAsync(string refreshToken)
    {
        var user = await _db.Users.FirstOrDefaultAsync(u =>
            u.RefreshToken == refreshToken && u.RefreshTokenExpiry > DateTime.UtcNow);

        if (user is null)
            throw new UnauthorizedAccessException("Invalid or expired refresh token.");

        var roles = await _userManager.GetRolesAsync(user);
        var newAccessToken = _tokenService.GenerateAccessToken(user, roles);
        var newRefreshToken = _tokenService.GenerateRefreshToken();

        user.RefreshToken = newRefreshToken;
        user.RefreshTokenExpiry = DateTime.UtcNow.AddDays(_config.GetValue<int>("Jwt:RefreshExpiryDays", 7));
        await _userManager.UpdateAsync(user);

        return (new AuthResponseDto
        {
            AccessToken = newAccessToken,
            ExpiresIn = _config.GetValue<int>("Jwt:ExpiresInMinutes", 15) * 60,
            User = MapUserDto(user, roles),
        }, newRefreshToken);
    }

    public async Task LogoutAsync(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user is null) return;
        user.RefreshToken = null;
        user.RefreshTokenExpiry = null;
        await _userManager.UpdateAsync(user);
    }

    private static UserDto MapUserDto(ApplicationUser user, IList<string> roles) => new()
    {
        Id = user.Id,
        Email = user.Email!,
        DisplayName = user.DisplayName,
        Roles = roles,
    };
}
