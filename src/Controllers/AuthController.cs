using Api.DTOs.Auth;
using Api.DTOs.Common;
using Api.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Api.Controllers;

/// <summary>Authentication — register, login, token refresh, and logout</summary>
[ApiController]
[Route("api/v1/auth")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly ILogger<AuthController> _logger;

    private static readonly CookieOptions RefreshCookieOptions = new()
    {
        HttpOnly = true,
        Secure = true,
        SameSite = SameSiteMode.Strict,
        MaxAge = TimeSpan.FromDays(7),
    };

    public AuthController(IAuthService authService, ILogger<AuthController> logger)
    {
        _authService = authService;
        _logger = logger;
    }

    /// <summary>Register a new user account</summary>
    [HttpPost("register")]
    [ProducesResponseType(typeof(ApiResponse<AuthResponseDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Register([FromBody] RegisterRequestDto dto)
    {
        var result = await _authService.RegisterAsync(dto);
        _logger.LogInformation("User registered: {Email}", dto.Email);
        return StatusCode(201, ApiResponse<AuthResponseDto>.Success(result));
    }

    /// <summary>Login with email and password</summary>
    [HttpPost("login")]
    [ProducesResponseType(typeof(ApiResponse<AuthResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login([FromBody] LoginRequestDto dto)
    {
        var (auth, refreshToken) = await _authService.LoginAsync(dto);
        Response.Cookies.Append("refreshToken", refreshToken, RefreshCookieOptions);
        return Ok(ApiResponse<AuthResponseDto>.Success(auth));
    }

    /// <summary>Exchange a refresh token for a new access token</summary>
    [HttpPost("refresh")]
    [ProducesResponseType(typeof(ApiResponse<AuthResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Refresh()
    {
        var refreshToken = Request.Cookies["refreshToken"];
        if (string.IsNullOrEmpty(refreshToken))
            return Unauthorized(ApiResponse<object>.Fail("No refresh token provided."));

        var (auth, newRefreshToken) = await _authService.RefreshAsync(refreshToken);
        Response.Cookies.Append("refreshToken", newRefreshToken, RefreshCookieOptions);
        return Ok(ApiResponse<AuthResponseDto>.Success(auth));
    }

    /// <summary>Logout and invalidate refresh token</summary>
    [HttpPost("logout")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Logout()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        await _authService.LogoutAsync(userId);
        Response.Cookies.Delete("refreshToken");
        return NoContent();
    }
}
