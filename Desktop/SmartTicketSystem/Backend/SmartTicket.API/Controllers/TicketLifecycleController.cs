using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartTicket.API.DTOs.Response;
using SmartTicket.API.Services;

namespace SmartTicket.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class TicketLifecycleController : ControllerBase
    {
        private readonly ITicketLifecycleService _lifecycleService;

        public TicketLifecycleController(ITicketLifecycleService lifecycleService)
        {
            _lifecycleService = lifecycleService;
        }

        [HttpPost("{ticketId}/start")]
        [Authorize(Roles = "Admin,SupportManager,SupportAgent")]
        public async Task<ActionResult<ApiResponseDto<object>>> StartWork(int ticketId)
        {
            try
            {
                var userId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value);
                var ticket = await _lifecycleService.MoveToInProgress(ticketId, userId);
                
                return Ok(new ApiResponseDto<object>
                {
                    Success = true,
                    Message = "Ticket moved to In Progress",
                    Data = ticket
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new ApiResponseDto<object> { Success = false, Message = ex.Message });
            }
        }

        [HttpPost("{ticketId}/resolve")]
        [Authorize(Roles = "Admin,SupportManager,SupportAgent")]
        public async Task<ActionResult<ApiResponseDto<object>>> Resolve(int ticketId, [FromBody] string resolutionNotes)
        {
            try
            {
                var userId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value);
                var ticket = await _lifecycleService.ResolveTicket(ticketId, resolutionNotes, userId);
                
                return Ok(new ApiResponseDto<object>
                {
                    Success = true,
                    Message = "Ticket resolved",
                    Data = ticket
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new ApiResponseDto<object> { Success = false, Message = ex.Message });
            }
        }

        [HttpPost("{ticketId}/close")]
        [Authorize(Roles = "Admin,SupportManager")]
        public async Task<ActionResult<ApiResponseDto<object>>> Close(int ticketId)
        {
            try
            {
                var userId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value);
                var ticket = await _lifecycleService.CloseTicket(ticketId, userId);
                
                return Ok(new ApiResponseDto<object>
                {
                    Success = true,
                    Message = "Ticket closed",
                    Data = ticket
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new ApiResponseDto<object> { Success = false, Message = ex.Message });
            }
        }

        [HttpPost("{ticketId}/reopen")]
        public async Task<ActionResult<ApiResponseDto<object>>> Reopen(int ticketId, [FromBody] string reason)
        {
            try
            {
                var userId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value);
                var ticket = await _lifecycleService.ReopenTicket(ticketId, reason, userId);
                
                return Ok(new ApiResponseDto<object>
                {
                    Success = true,
                    Message = "Ticket reopened",
                    Data = ticket
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new ApiResponseDto<object> { Success = false, Message = ex.Message });
            }
        }

        [HttpPost("{ticketId}/cancel")]
        [Authorize(Roles = "Admin,SupportManager")]
        public async Task<ActionResult<ApiResponseDto<object>>> Cancel(int ticketId, [FromBody] string reason)
        {
            try
            {
                var userId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value);
                var ticket = await _lifecycleService.CancelTicket(ticketId, reason, userId);
                
                return Ok(new ApiResponseDto<object>
                {
                    Success = true,
                    Message = "Ticket cancelled",
                    Data = ticket
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new ApiResponseDto<object> { Success = false, Message = ex.Message });
            }
        }

        [HttpGet("allowed-statuses/{currentStatus}")]
        public async Task<ActionResult<ApiResponseDto<List<string>>>> GetAllowedStatuses(string currentStatus)
        {
            var statuses = await _lifecycleService.GetAllowedNextStatuses(currentStatus);
            return Ok(new ApiResponseDto<List<string>>
            {
                Success = true,
                Message = "Allowed statuses retrieved",
                Data = statuses
            });
        }
    }
}