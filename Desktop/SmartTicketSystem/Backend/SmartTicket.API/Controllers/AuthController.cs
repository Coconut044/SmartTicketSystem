using Microsoft.AspNetCore.Mvc;
using SmartTicket.API.DTOs.Request;
using SmartTicket.API.DTOs.Response;
using SmartTicket.API.Services;

namespace SmartTicket.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("login")]
        public async Task<ActionResult<ApiResponseDto<AuthResponseDto>>> Login([FromBody] LoginRequestDto request)
        {
            try
            {
                var result = await _authService.LoginAsync(request);
                return Ok(new ApiResponseDto<AuthResponseDto>
                {
                    Success = true,
                    Message = "Login successful",
                    Data = result
                });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new ApiResponseDto<AuthResponseDto>
                {
                    Success = false,
                    Message = ex.Message
                });
            }
        }

        [HttpPost("register")]
        public async Task<ActionResult<ApiResponseDto<AuthResponseDto>>> Register([FromBody] RegisterRequestDto request)
        {
            try
            {
                var result = await _authService.RegisterAsync(request);
                return Ok(new ApiResponseDto<AuthResponseDto>
                {
                    Success = true,
                    Message = "Registration successful",
                    Data = result
                });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new ApiResponseDto<AuthResponseDto>
                {
                    Success = false,
                    Message = ex.Message
                });
            }
        }
    }
}