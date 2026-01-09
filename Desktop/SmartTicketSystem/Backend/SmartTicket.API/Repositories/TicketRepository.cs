using Microsoft.EntityFrameworkCore;
using SmartTicket.API.Data;
using SmartTicket.API.Models.Entities;

namespace SmartTicket.API.Repositories
{
    public class TicketRepository : ITicketRepository
    {
        private readonly ApplicationDbContext _context;

        public TicketRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Ticket?> GetByIdAsync(int id)
        {
            return await _context.Tickets
                .Include(t => t.Category)
                .Include(t => t.CreatedBy)
                .Include(t => t.AssignedTo)
                .FirstOrDefaultAsync(t => t.Id == id);
        }

        public async Task<Ticket?> GetByIdWithDetailsAsync(int id)
        {
            return await _context.Tickets
                .Include(t => t.Category)
                .Include(t => t.CreatedBy)
                .Include(t => t.AssignedTo)
                .Include(t => t.Comments.OrderByDescending(c => c.CreatedAt))
                    .ThenInclude(c => c.User)
                .Include(t => t.History.OrderByDescending(h => h.CreatedAt))
                    .ThenInclude(h => h.User)
                .FirstOrDefaultAsync(t => t.Id == id);
        }

        public async Task<IEnumerable<Ticket>> GetAllAsync(int pageNumber, int pageSize, string? status, string? priority, int? categoryId, int? assignedToId, int? createdById)
        {
            var query = _context.Tickets
                .Include(t => t.Category)
                .Include(t => t.CreatedBy)
                .Include(t => t.AssignedTo)
                .AsQueryable();

            if (!string.IsNullOrEmpty(status))
                query = query.Where(t => t.Status == status);

            if (!string.IsNullOrEmpty(priority))
                query = query.Where(t => t.Priority == priority);

            if (categoryId.HasValue)
                query = query.Where(t => t.CategoryId == categoryId.Value);

            if (assignedToId.HasValue)
                query = query.Where(t => t.AssignedToId == assignedToId.Value);

            if (createdById.HasValue)
                query = query.Where(t => t.CreatedById == createdById.Value);

            return await query
                .OrderByDescending(t => t.CreatedAt)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<int> GetTotalCountAsync(string? status, string? priority, int? categoryId, int? assignedToId, int? createdById)
        {
            var query = _context.Tickets.AsQueryable();

            if (!string.IsNullOrEmpty(status))
                query = query.Where(t => t.Status == status);

            if (!string.IsNullOrEmpty(priority))
                query = query.Where(t => t.Priority == priority);

            if (categoryId.HasValue)
                query = query.Where(t => t.CategoryId == categoryId.Value);

            if (assignedToId.HasValue)
                query = query.Where(t => t.AssignedToId == assignedToId.Value);

            if (createdById.HasValue)
                query = query.Where(t => t.CreatedById == createdById.Value);

            return await query.CountAsync();
        }

        public async Task<Ticket> CreateAsync(Ticket ticket)
        {
            _context.Tickets.Add(ticket);
            await _context.SaveChangesAsync();
            return await GetByIdAsync(ticket.Id) ?? ticket;
        }

        public async Task<Ticket> UpdateAsync(Ticket ticket)
        {
            ticket.UpdatedAt = DateTime.UtcNow;
            _context.Tickets.Update(ticket);
            await _context.SaveChangesAsync();
            return await GetByIdAsync(ticket.Id) ?? ticket;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var ticket = await GetByIdAsync(id);
            if (ticket == null) return false;

            _context.Tickets.Remove(ticket);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<IEnumerable<Ticket>> GetOverdueTicketsAsync()
        {
            var now = DateTime.UtcNow;
            return await _context.Tickets
                .Include(t => t.Category)
                .Where(t => t.DueDate.HasValue && 
                           t.DueDate.Value < now && 
                           t.Status != "Resolved" && 
                           t.Status != "Closed")
                .ToListAsync();
        }

        public async Task<Dictionary<string, int>> GetTicketCountByStatusAsync()
        {
            return await _context.Tickets
                .GroupBy(t => t.Status)
                .Select(g => new { Status = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.Status, x => x.Count);
        }

        public async Task<Dictionary<string, int>> GetTicketCountByPriorityAsync()
        {
            return await _context.Tickets
                .GroupBy(t => t.Priority)
                .Select(g => new { Priority = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.Priority, x => x.Count);
        }

        public async Task<Dictionary<string, int>> GetTicketCountByCategoryAsync()
        {
            return await _context.Tickets
                .Include(t => t.Category)
                .GroupBy(t => t.Category.Name)
                .Select(g => new { Category = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.Category, x => x.Count);
        }

        public async Task<double> GetAverageResolutionTimeAsync()
        {
            var resolvedTickets = await _context.Tickets
                .Where(t => t.ResolvedAt.HasValue)
                .Select(t => new
                {
                    t.CreatedAt,
                    t.ResolvedAt
                })
                .ToListAsync();

            if (!resolvedTickets.Any())
                return 0;

            var totalHours = resolvedTickets
                .Sum(t => (t.ResolvedAt!.Value - t.CreatedAt).TotalHours);

            return totalHours / resolvedTickets.Count;
        }
    }
}
