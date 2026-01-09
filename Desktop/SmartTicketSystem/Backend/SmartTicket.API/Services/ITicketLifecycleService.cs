using SmartTicket.API.Models.Entities;

namespace SmartTicket.API.Services
{
    public interface ITicketLifecycleService
    {
        Task<Ticket> MoveToInProgress(int ticketId, int userId);
        Task<Ticket> ResolveTicket(int ticketId, string resolutionNotes, int userId);
        Task<Ticket> CloseTicket(int ticketId, int userId);
        Task<Ticket> ReopenTicket(int ticketId, string reason, int userId);
        Task<Ticket> CancelTicket(int ticketId, string reason, int userId);
        Task<bool> ValidateStatusTransition(string currentStatus, string newStatus);
        Task<List<string>> GetAllowedNextStatuses(string currentStatus);
    }
}
