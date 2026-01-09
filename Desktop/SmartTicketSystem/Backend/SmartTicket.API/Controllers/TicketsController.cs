using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using SmartTicket.API.DTOs.Request;
using SmartTicket.API.DTOs.Response;
using SmartTicket.API.Services;

namespace SmartTicket.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class TicketsController : ControllerBase
    {
        private readonly ITicketService _ticketService;

        public TicketsController(ITicketService ticketService)
        {
            _ticketService = ticketService;
        }

        /// <summary>
        /// Get tickets with role-based filtering
        /// - Admin/SupportManager: See all tickets
        /// - SupportAgent: See only assigned tickets
        /// - EndUser: See only their own tickets
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<ApiResponseDto<PagedResultDto<TicketDto>>>> GetTickets(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string? status = null,
            [FromQuery] string? priority = null,
            [FromQuery] int? categoryId = null,
            [FromQuery] int? assignedToId = null,
            [FromQuery] int? createdById = null)
        {
            var userId = GetCurrentUserId();
            var userRole = GetCurrentUserRole();

            // ✅ RBAC: Filter based on role
            if (userRole == "EndUser")
            {
                // End users can ONLY see their own tickets
                createdById = userId;
            }
            else if (userRole == "SupportAgent")
            {
                // Support agents can ONLY see tickets assigned to them
                assignedToId = userId;
            }
            // Admin and SupportManager can see all tickets (no filter)

            var result = await _ticketService.GetAllTicketsAsync(
                pageNumber, pageSize, status, priority, categoryId, assignedToId, createdById);
            
            return Ok(new ApiResponseDto<PagedResultDto<TicketDto>>
            {
                Success = true,
                Message = "Tickets retrieved successfully",
                Data = result
            });
        }

        /// <summary>
        /// Get single ticket with role-based access
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<ApiResponseDto<TicketDetailDto>>> GetTicket(int id)
        {
            var ticket = await _ticketService.GetTicketByIdAsync(id);
            if (ticket == null)
            {
                return NotFound(new ApiResponseDto<TicketDetailDto>
                {
                    Success = false,
                    Message = "Ticket not found"
                });
            }

            // ✅ RBAC: Check if user has access to this ticket
            var userId = GetCurrentUserId();
            var userRole = GetCurrentUserRole();

            if (userRole == "EndUser" && ticket.CreatedById != userId)
            {
                return Forbid(); // End user trying to access someone else's ticket
            }

            if (userRole == "SupportAgent" && ticket.AssignedToId != userId)
            {
                return Forbid(); // Agent trying to access unassigned ticket
            }

            // Admin and SupportManager can view all tickets

            return Ok(new ApiResponseDto<TicketDetailDto>
            {
                Success = true,
                Message = "Ticket retrieved successfully",
                Data = ticket
            });
        }

        /// <summary>
        /// Create ticket - All authenticated users can create
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<ApiResponseDto<TicketDto>>> CreateTicket([FromBody] CreateTicketDto dto)
        {
            var userId = GetCurrentUserId();
            var ticket = await _ticketService.CreateTicketAsync(dto, userId);
            
            return CreatedAtAction(nameof(GetTicket), new { id = ticket.Id }, new ApiResponseDto<TicketDto>
            {
                Success = true,
                Message = "Ticket created successfully",
                Data = ticket
            });
        }

        /// <summary>
        /// Update ticket - Role-based restrictions
        /// - EndUser: Can update only their own tickets (and only if status is "Created")
        /// - SupportAgent: Can update only assigned tickets
        /// - SupportManager/Admin: Can update any ticket
        /// </summary>
        [HttpPut("{id}")]
        public async Task<ActionResult<ApiResponseDto<TicketDto>>> UpdateTicket(int id, [FromBody] UpdateTicketDto dto)
        {
            try
            {
                var userId = GetCurrentUserId();
                var userRole = GetCurrentUserRole();

                // Get existing ticket to check permissions
                var existingTicket = await _ticketService.GetTicketByIdAsync(id);
                if (existingTicket == null)
                {
                    return NotFound(new ApiResponseDto<TicketDto>
                    {
                        Success = false,
                        Message = "Ticket not found"
                    });
                }

                // ✅ RBAC: Permission checks
                if (userRole == "EndUser")
                {
                    if (existingTicket.CreatedById != userId)
                    {
                        return Forbid(); // Can't update someone else's ticket
                    }
                    if (existingTicket.Status != "Created")
                    {
                        return BadRequest(new ApiResponseDto<TicketDto>
                        {
                            Success = false,
                            Message = "You can only update tickets in 'Created' status"
                        });
                    }
                }
                else if (userRole == "SupportAgent")
                {
                    if (existingTicket.AssignedToId != userId)
                    {
                        return Forbid(); // Can't update tickets not assigned to them
                    }
                }
                // Admin and SupportManager can update any ticket

                var ticket = await _ticketService.UpdateTicketAsync(id, dto, userId);
                
                return Ok(new ApiResponseDto<TicketDto>
                {
                    Success = true,
                    Message = "Ticket updated successfully",
                    Data = ticket
                });
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(new ApiResponseDto<TicketDto>
                {
                    Success = false,
                    Message = ex.Message
                });
            }
        }

        /// <summary>
        /// Delete ticket - Admin only
        /// </summary>
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<ApiResponseDto<bool>>> DeleteTicket(int id)
        {
            var result = await _ticketService.DeleteTicketAsync(id);
            if (!result)
            {
                return NotFound(new ApiResponseDto<bool>
                {
                    Success = false,
                    Message = "Ticket not found"
                });
            }

            return Ok(new ApiResponseDto<bool>
            {
                Success = true,
                Message = "Ticket deleted successfully",
                Data = true
            });
        }

        /// <summary>
        /// Assign ticket - Admin and SupportManager only
        /// </summary>
        [HttpPost("{id}/assign")]
        [Authorize(Roles = "Admin,SupportManager")]
        public async Task<ActionResult<ApiResponseDto<TicketDto>>> AssignTicket(
            int id, 
            [FromBody] AssignTicketRequestDto request)
        {
            try
            {
                var userId = GetCurrentUserId();
                var ticket = await _ticketService.AssignTicketAsync(id, request.UserId, userId);
                
                return Ok(new ApiResponseDto<TicketDto>
                {
                    Success = true,
                    Message = "Ticket assigned successfully",
                    Data = ticket
                });
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(new ApiResponseDto<TicketDto>
                {
                    Success = false,
                    Message = ex.Message
                });
            }
        }

        /// <summary>
        /// Update ticket status with role-based rules
        /// - EndUser: Can only reopen their own closed tickets
        /// - SupportAgent: Can update status of assigned tickets (Created → In Progress → Resolved)
        /// - SupportManager: Can update any status including closing tickets
        /// - Admin: Full control
        /// </summary>
        [HttpPut("{id}/status")]
        public async Task<ActionResult<ApiResponseDto<TicketDto>>> UpdateStatus(
            int id, 
            [FromBody] UpdateStatusRequestDto request)
        {
            try
            {
                var userId = GetCurrentUserId();
                var userRole = GetCurrentUserRole();

                // Get existing ticket
                var existingTicket = await _ticketService.GetTicketByIdAsync(id);
                if (existingTicket == null)
                {
                    return NotFound(new ApiResponseDto<TicketDto>
                    {
                        Success = false,
                        Message = "Ticket not found"
                    });
                }

                // ✅ RBAC: Status change permissions
                if (userRole == "EndUser")
                {
                    if (existingTicket.CreatedById != userId)
                    {
                        return Forbid();
                    }
                    // End users can only reopen closed tickets or cancel created tickets
                    if (!(existingTicket.Status == "Closed" && request.Status == "Created") &&
                        !(existingTicket.Status == "Created" && request.Status == "Cancelled"))
                    {
                        return BadRequest(new ApiResponseDto<TicketDto>
                        {
                            Success = false,
                            Message = "You can only reopen closed tickets or cancel newly created tickets"
                        });
                    }
                }
                else if (userRole == "SupportAgent")
                {
                    if (existingTicket.AssignedToId != userId)
                    {
                        return Forbid();
                    }
                    // Agents can: Assigned → InProgress → Resolved
                    var allowedTransitions = new[]
                    {
                        ("Assigned", "InProgress"),
                        ("InProgress", "Resolved"),
                        ("InProgress", "Assigned") // Can move back if needed
                    };
                    
                    if (!allowedTransitions.Any(t => t.Item1 == existingTicket.Status && t.Item2 == request.Status))
                    {
                        return BadRequest(new ApiResponseDto<TicketDto>
                        {
                            Success = false,
                            Message = "Invalid status transition for your role"
                        });
                    }
                }
                // SupportManager and Admin can change any status

                var ticket = await _ticketService.UpdateTicketStatusAsync(id, request.Status, userId);
                
                return Ok(new ApiResponseDto<TicketDto>
                {
                    Success = true,
                    Message = "Status updated successfully",
                    Data = ticket
                });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new ApiResponseDto<TicketDto>
                {
                    Success = false,
                    Message = ex.Message
                });
            }
        }

        /// <summary>
        /// Add comment - Users with access to ticket can comment
        /// </summary>
        [HttpPost("{id}/comments")]
        public async Task<ActionResult<ApiResponseDto<CommentDto>>> AddComment(
            int id, 
            [FromBody] AddCommentDto dto)
        {
            try
            {
                var userId = GetCurrentUserId();
                var userRole = GetCurrentUserRole();

                // Get ticket to verify access
                var ticket = await _ticketService.GetTicketByIdAsync(id);
                if (ticket == null)
                {
                    return NotFound(new ApiResponseDto<CommentDto>
                    {
                        Success = false,
                        Message = "Ticket not found"
                    });
                }

                // ✅ RBAC: Check if user has access to this ticket
                if (userRole == "EndUser" && ticket.CreatedById != userId)
                {
                    return Forbid();
                }
                else if (userRole == "SupportAgent" && ticket.AssignedToId != userId)
                {
                    return Forbid();
                }
                // Admin and SupportManager can comment on any ticket

                var comment = await _ticketService.AddCommentAsync(id, dto, userId);
                
                return Ok(new ApiResponseDto<CommentDto>
                {
                    Success = true,
                    Message = "Comment added successfully",
                    Data = comment
                });
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(new ApiResponseDto<CommentDto>
                {
                    Success = false,
                    Message = ex.Message
                });
            }
        }

        /// <summary>
        /// Get comments for a ticket - Users with ticket access only
        /// NOTE: This endpoint is handled by CommentsController
        /// Keeping this here for reference but using CommentService
        /// </summary>
        [HttpGet("{id}/comments")]
        public async Task<ActionResult<ApiResponseDto<List<CommentDto>>>> GetComments(int id)
        {
            var userId = GetCurrentUserId();
            var userRole = GetCurrentUserRole();

            // Get ticket to verify access
            var ticket = await _ticketService.GetTicketByIdAsync(id);
            if (ticket == null)
            {
                return NotFound(new ApiResponseDto<List<CommentDto>>
                {
                    Success = false,
                    Message = "Ticket not found"
                });
            }

            // ✅ RBAC: Check access
            if (userRole == "EndUser" && ticket.CreatedById != userId)
            {
                return Forbid();
            }
            else if (userRole == "SupportAgent" && ticket.AssignedToId != userId)
            {
                return Forbid();
            }

            // Comments are retrieved through the ticket detail
            // The frontend should use the comments included in TicketDetailDto
            // Or call a separate CommentsController if you create one
            
            return Ok(new ApiResponseDto<List<CommentDto>>
            {
                Success = true,
                Message = "Use GET /api/tickets/{id} to get ticket with comments, or create a separate CommentsController",
                Data = new List<CommentDto>()
            });
        }

        // Helper methods
        private int GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            return int.Parse(userIdClaim!.Value);
        }

        private string GetCurrentUserRole()
        {
            var roleClaim = User.FindFirst(ClaimTypes.Role);
            return roleClaim?.Value ?? "EndUser";
        }
    }

    // DTO for request bodies that were using primitives
    public class AssignTicketRequestDto
    {
        public int UserId { get; set; }
    }

    public class UpdateStatusRequestDto
    {
        public string Status { get; set; } = string.Empty;
    }
}