using SmartTicket.API.DTOs.Request;
using SmartTicket.API.DTOs.Response;

namespace SmartTicket.API.Services
{
    public interface ITicketService
    {
        Task<PagedResultDto<TicketDto>> GetAllTicketsAsync(
            int pageNumber,
            int pageSize,
            string? status,
            string? priority,
            int? categoryId,
            int? assignedToId,
            int? createdById);

        Task<TicketDetailDto?> GetTicketByIdAsync(int id);

        Task<TicketDto> CreateTicketAsync(CreateTicketDto dto, int userId);

        Task<TicketDto> UpdateTicketAsync(int id, UpdateTicketDto dto, int userId);

        Task<bool> DeleteTicketAsync(int id);

        Task<TicketDto> AssignTicketAsync(int ticketId, int assignedToId, int userId);

        Task<TicketDto> UpdateTicketStatusAsync(int ticketId, string status, int userId);

        Task<CommentDto> AddCommentAsync(int ticketId, AddCommentDto dto, int userId);

        Task<DashboardStatsDto> GetDashboardStatsAsync();

        // Ticket lifecycle extensions
        Task<TicketDto> ReopenTicketAsync(int ticketId, int userId);
        Task<TicketDto> CancelTicketAsync(int ticketId, int userId);
    }
}
