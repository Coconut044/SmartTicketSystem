using SmartTicket.API.Models.Entities;

namespace SmartTicket.API.Services
{
    public interface IAssignmentService
    {
        Task<Ticket> AssignTicketManually(int ticketId, int agentId, int assignedBy);
        Task<Ticket> AssignTicketAutomatically(int ticketId);
        Task<List<Ticket>> GetUnassignedTickets();
        Task<Dictionary<int, int>> GetAgentWorkload();
        Task<List<Ticket>> CheckAndEscalateOverdueTickets();
        Task<bool> CheckSlaBreached(Ticket ticket);
    }
}