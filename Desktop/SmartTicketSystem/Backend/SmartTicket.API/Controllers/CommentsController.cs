using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using SmartTicket.API.DTOs.Request;
using SmartTicket.API.DTOs.Response;
using SmartTicket.API.Services;

namespace SmartTicket.API.Controllers
{
    
    [ApiController]
    [Route("api/tickets/{ticketId}/comments")]
    [Authorize]
    public class CommentsController : ControllerBase
    {
        private readonly ICommentService _commentService;
        private readonly ITicketService _ticketService;

        public CommentsController(ICommentService commentService, ITicketService ticketService)
        {
            _commentService = commentService;
            _ticketService = ticketService;
        }

        
        
        [HttpGet]
        public async Task<ActionResult<ApiResponseDto<List<CommentDto>>>> GetComments(int ticketId)
        {
            var userId = GetCurrentUserId();
            var userRole = GetCurrentUserRole();

            
            var ticket = await _ticketService.GetTicketByIdAsync(ticketId);
            if (ticket == null)
            {
                return NotFound(new ApiResponseDto<List<CommentDto>>
                {
                    Success = false,
                    Message = "Ticket not found"
                });
            }

            
            if (userRole == "EndUser" && ticket.CreatedById != userId)
            {
                return Forbid();
            }
            else if (userRole == "SupportAgent" && ticket.AssignedToId != userId)
            {
                return Forbid();
            }
            

            var comments = await _commentService.GetCommentsForTicketAsync(ticketId);
            
            return Ok(new ApiResponseDto<List<CommentDto>>
            {
                Success = true,
                Message = "Comments retrieved successfully",
                Data = comments
            });
        }

        
        [HttpPost]
        public async Task<ActionResult<ApiResponseDto<CommentDto>>> AddComment(
            int ticketId, 
            [FromBody] AddCommentDto dto)
        {
            try
            {
                var userId = GetCurrentUserId();
                var userRole = GetCurrentUserRole();

            
                var ticket = await _ticketService.GetTicketByIdAsync(ticketId);
                if (ticket == null)
                {
                    return NotFound(new ApiResponseDto<CommentDto>
                    {
                        Success = false,
                        Message = "Ticket not found"
                    });
                }

                
                if (userRole == "EndUser" && ticket.CreatedById != userId)
                {
                    return Forbid();
                }
                else if (userRole == "SupportAgent" && ticket.AssignedToId != userId)
                {
                    return Forbid();
                }
                // Admin and SupportManager can comment on any ticket

                var comment = await _commentService.AddCommentAsync(ticketId, dto, userId);
                
                return Ok(new ApiResponseDto<CommentDto>
                {
                    Success = true,
                    Message = "Comment added successfully",
                    Data = comment
                });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new ApiResponseDto<CommentDto>
                {
                    Success = false,
                    Message = ex.Message
                });
            }
        }

        
        
    
        [HttpGet("{commentId}")]
        public async Task<ActionResult<ApiResponseDto<CommentDto>>> GetComment(int ticketId, int commentId)
        {
            var userId = GetCurrentUserId();
            var userRole = GetCurrentUserRole();

            
            var ticket = await _ticketService.GetTicketByIdAsync(ticketId);
            if (ticket == null)
            {
                return NotFound(new ApiResponseDto<CommentDto>
                {
                    Success = false,
                    Message = "Ticket not found"
                });
            }

            
            if (userRole == "EndUser" && ticket.CreatedById != userId)
            {
                return Forbid();
            }
            else if (userRole == "SupportAgent" && ticket.AssignedToId != userId)
            {
                return Forbid();
            }

            var comment = await _commentService.GetCommentByIdAsync(commentId);
            if (comment == null)
            {
                return NotFound(new ApiResponseDto<CommentDto>
                {
                    Success = false,
                    Message = "Comment not found"
                });
            }

            
            if (comment.TicketId != ticketId)
            {
                return BadRequest(new ApiResponseDto<CommentDto>
                {
                    Success = false,
                    Message = "Comment does not belong to this ticket"
                });
            }

            return Ok(new ApiResponseDto<CommentDto>
            {
                Success = true,
                Message = "Comment retrieved successfully",
                Data = comment
            });
        }

      
        [HttpPut("{commentId}")]
        public async Task<ActionResult<ApiResponseDto<CommentDto>>> UpdateComment(
            int ticketId,
            int commentId,
            [FromBody] UpdateCommentDto dto)
        {
            try
            {
                var userId = GetCurrentUserId();
                var userRole = GetCurrentUserRole();

               
                var comment = await _commentService.GetCommentByIdAsync(commentId);
                if (comment == null)
                {
                    return NotFound(new ApiResponseDto<CommentDto>
                    {
                        Success = false,
                        Message = "Comment not found"
                    });
                }

               
                if (comment.TicketId != ticketId)
                {
                    return BadRequest(new ApiResponseDto<CommentDto>
                    {
                        Success = false,
                        Message = "Comment does not belong to this ticket"
                    });
                }

              
                if (userRole != "Admin" && comment.UserId != userId)
                {
                    return Forbid();
                }

                
                var commentAge = DateTime.UtcNow - comment.CreatedAt;
                if (userRole != "Admin" && commentAge.TotalMinutes > 15)
                {
                    return BadRequest(new ApiResponseDto<CommentDto>
                    {
                        Success = false,
                        Message = "Comments can only be edited within 15 minutes of creation"
                    });
                }

                var updatedComment = await _commentService.UpdateCommentAsync(commentId, dto);
                
                return Ok(new ApiResponseDto<CommentDto>
                {
                    Success = true,
                    Message = "Comment updated successfully",
                    Data = updatedComment
                });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new ApiResponseDto<CommentDto>
                {
                    Success = false,
                    Message = ex.Message
                });
            }
        }


        [HttpDelete("{commentId}")]
        public async Task<ActionResult<ApiResponseDto<bool>>> DeleteComment(int ticketId, int commentId)
        {
            var userId = GetCurrentUserId();
            var userRole = GetCurrentUserRole();

            
            var comment = await _commentService.GetCommentByIdAsync(commentId);
            if (comment == null)
            {
                return NotFound(new ApiResponseDto<bool>
                {
                    Success = false,
                    Message = "Comment not found"
                });
            }

            // Verify comment belongs to this ticket
            if (comment.TicketId != ticketId)
            {
                return BadRequest(new ApiResponseDto<bool>
                {
                    Success = false,
                    Message = "Comment does not belong to this ticket"
                });
            }

            // ✅ RBAC: Users can only delete their own comments, Admin can delete any
            if (userRole != "Admin" && comment.UserId != userId)
            {
                return Forbid();
            }

            var result = await _commentService.DeleteCommentAsync(commentId);
            
            return Ok(new ApiResponseDto<bool>
            {
                Success = true,
                Message = "Comment deleted successfully",
                Data = result
            });
        }

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
}

   
    