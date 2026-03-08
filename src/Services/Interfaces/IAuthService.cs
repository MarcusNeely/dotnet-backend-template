using Api.DTOs.Auth;

namespace Api.Services.Interfaces;

public interface IAuthService
{
    Task<AuthResponseDto> RegisterAsync(RegisterRequestDto dto);
    Task<(AuthResponseDto Auth, string RefreshToken)> LoginAsync(LoginRequestDto dto);
    Task<(AuthResponseDto Auth, string RefreshToken)> RefreshAsync(string refreshToken);
    Task LogoutAsync(string userId);
}
