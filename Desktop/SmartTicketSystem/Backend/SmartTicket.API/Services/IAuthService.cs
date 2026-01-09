using SmartTicket.API.DTOs.Request;
using SmartTicket.API.DTOs.Response;

namespace SmartTicket.API.Services
{
    public interface IAuthService
    {
        Task<AuthResponseDto> LoginAsync(LoginRequestDto request);
        Task<AuthResponseDto> RegisterAsync(RegisterRequestDto request);
        string GenerateJwtToken(int userId, string email, string role);
    }
}