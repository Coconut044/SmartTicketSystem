using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartTicket.API.Services;

namespace SmartTicket.API.Controllers
{
    [ApiController]
    [Route("api/assignments")]
    [Authorize]
    public class AssignmentController : ControllerBase
    {
        private readonly IAssignmentService _assignmentService;

        public AssignmentController(IAssignmentService assignmentService)
        {
            _assignmentService = assignmentService;
        }

        [HttpPost("{ticketId}/assign/{agentId}")]
        [Authorize(Roles = "Admin,SupportManager")]
        public async Task<IActionResult> AssignTicketManually(
            int ticketId,
            int agentId)
        {
            // assignedBy taken from logged-in user
            var assignedBy = int.Parse(User.FindFirst("id")!.Value);

            var ticket = await _assignmentService.AssignTicketManually(
                ticketId,
                agentId,
                assignedBy
            );

            return Ok(ticket);
        }

    
        [HttpPost("{ticketId}/auto-assign")]
        [Authorize(Roles = "Admin,SupportManager")]
        public async Task<IActionResult> AssignTicketAutomatically(int ticketId)
        {
            var ticket = await _assignmentService.AssignTicketAutomatically(ticketId);
            return Ok(ticket);
        }

        
        [HttpGet("unassigned")]
        [Authorize(Roles = "Admin,SupportManager")]
        public async Task<IActionResult> GetUnassignedTickets()
        {
            var tickets = await _assignmentService.GetUnassignedTickets();
            return Ok(tickets);
        }

        
        [HttpGet("agent-workload")]
        [Authorize(Roles = "Admin,SupportManager")]
        public async Task<IActionResult> GetAgentWorkload()
        {
            var workload = await _assignmentService.GetAgentWorkload();
            return Ok(workload);
        }
    }
}
