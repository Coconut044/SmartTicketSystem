using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartTicket.API.DTOs.Response;
using SmartTicket.API.Repositories;

namespace SmartTicket.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class NotificationsController : ControllerBase
    {
        private readonly INotificationRepository _notificationRepository;

        public NotificationsController(INotificationRepository notificationRepository)
        {
            _notificationRepository = notificationRepository;
        }

        [HttpGet]
        public async Task<ActionResult<ApiResponseDto<List<NotificationDto>>>> GetNotifications([FromQuery] bool unreadOnly = false)
        {
            var userId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value);
            var notifications = await _notificationRepository.GetUserNotificationsAsync(userId, unreadOnly);

            var notificationDtos = notifications.Select(n => new NotificationDto
            {
                Id = n.Id,
                UserId = n.UserId,
                TicketId = n.TicketId,
                Title = n.Title,
                Message = n.Message,
                IsRead = n.IsRead,
                CreatedAt = n.CreatedAt
            }).ToList();

            return Ok(new ApiResponseDto<List<NotificationDto>>
            {
                Success = true,
                Message = "Notifications retrieved successfully",
                Data = notificationDtos
            });
        }

        [HttpGet("unread-count")]
        public async Task<ActionResult<ApiResponseDto<int>>> GetUnreadCount()
        {
            var userId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value);
            var count = await _notificationRepository.GetUnreadCountAsync(userId);

            return Ok(new ApiResponseDto<int>
            {
                Success = true,
                Message = "Unread count retrieved",
                Data = count
            });
        }

        [HttpPost("{notificationId}/read")]
        public async Task<ActionResult<ApiResponseDto<bool>>> MarkAsRead(int notificationId)
        {
            var result = await _notificationRepository.MarkAsReadAsync(notificationId);

            if (!result)
            {
                return NotFound(new ApiResponseDto<bool>
                {
                    Success = false,
                    Message = "Notification not found"
                });
            }

            return Ok(new ApiResponseDto<bool>
            {
                Success = true,
                Message = "Notification marked as read",
                Data = true
            });
        }

        [HttpPost("mark-all-read")]
        public async Task<ActionResult<ApiResponseDto<bool>>> MarkAllAsRead()
        {
            var userId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value);
            await _notificationRepository.MarkAllAsReadAsync(userId);

            return Ok(new ApiResponseDto<bool>
            {
                Success = true,
                Message = "All notifications marked as read",
                Data = true
            });
        }
    }
}