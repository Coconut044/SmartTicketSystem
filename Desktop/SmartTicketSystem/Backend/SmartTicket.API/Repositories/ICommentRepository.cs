using SmartTicket.API.Models.Entities;

namespace SmartTicket.API.Repositories
{
    public interface ICommentRepository
    {
        Task<TicketComment> AddCommentAsync(TicketComment comment);
        Task<List<TicketComment>> GetCommentsByTicketIdAsync(int ticketId);
        Task<List<TicketComment>> GetCommentsByTicketIdAsync(int ticketId, bool includeInternal);
        Task<TicketComment?> GetCommentByIdAsync(int commentId);
        Task<bool> DeleteCommentAsync(int commentId);
    }
}