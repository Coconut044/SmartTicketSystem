using SmartTicket.API.Models.Entities;

namespace SmartTicket.API.Repositories
{
    public interface ITicketRepository
    {
        Task<Ticket?> GetByIdAsync(int id);
        Task<Ticket?> GetByIdWithDetailsAsync(int id);
        Task<IEnumerable<Ticket>> GetAllAsync(int pageNumber, int pageSize, string? status, string? priority, int? categoryId, int? assignedToId, int? createdById);
        Task<int> GetTotalCountAsync(string? status, string? priority, int? categoryId, int? assignedToId, int? createdById);
        Task<Ticket> CreateAsync(Ticket ticket);
        Task<Ticket> UpdateAsync(Ticket ticket);
        Task<bool> DeleteAsync(int id);
        Task<IEnumerable<Ticket>> GetOverdueTicketsAsync();
        Task<Dictionary<string, int>> GetTicketCountByStatusAsync();
        Task<Dictionary<string, int>> GetTicketCountByPriorityAsync();
        Task<Dictionary<string, int>> GetTicketCountByCategoryAsync();
        Task<double> GetAverageResolutionTimeAsync();
    }
}