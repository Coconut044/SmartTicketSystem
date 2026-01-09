using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartTicket.API.DTOs.Response;
using SmartTicket.API.Services;

namespace SmartTicket.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class DashboardController : ControllerBase
    {
        private readonly ITicketService _ticketService;

        public DashboardController(ITicketService ticketService)
        {
            _ticketService = ticketService;
        }

        [HttpGet("stats")]
        public async Task<ActionResult<ApiResponseDto<DashboardStatsDto>>> GetDashboardStats()
        {
            var stats = await _ticketService.GetDashboardStatsAsync();
            
            return Ok(new ApiResponseDto<DashboardStatsDto>
            {
                Success = true,
                Message = "Dashboard statistics retrieved successfully",
                Data = stats
            });
        }
    }
}
