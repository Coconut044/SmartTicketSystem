using Microsoft.EntityFrameworkCore;
using SmartTicket.API.Data;
using SmartTicket.API.Models.Entities;

namespace SmartTicket.API.Repositories
{
    public class CommentRepository : ICommentRepository
    {
        private readonly ApplicationDbContext _context;

        public CommentRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<TicketComment> AddCommentAsync(TicketComment comment)
        {
            _context.TicketComments.Add(comment);
            await _context.SaveChangesAsync();
            return await GetCommentByIdAsync(comment.Id) ?? comment;
        }

        public async Task<List<TicketComment>> GetCommentsByTicketIdAsync(int ticketId)
        {
            return await _context.TicketComments
                .Include(c => c.User)
                .Where(c => c.TicketId == ticketId)
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync();
        }

        public async Task<List<TicketComment>> GetCommentsByTicketIdAsync(int ticketId, bool includeInternal)
        {
            var query = _context.TicketComments
                .Include(c => c.User)
                .Where(c => c.TicketId == ticketId);

            if (!includeInternal)
            {
                query = query.Where(c => !c.IsInternal);
            }

            return await query.OrderByDescending(c => c.CreatedAt).ToListAsync();
        }

        public async Task<TicketComment?> GetCommentByIdAsync(int commentId)
        {
            return await _context.TicketComments
                .Include(c => c.User)
                .FirstOrDefaultAsync(c => c.Id == commentId);
        }

        public async Task<bool> DeleteCommentAsync(int commentId)
        {
            var comment = await _context.TicketComments.FindAsync(commentId);
            if (comment == null) return false;

            _context.TicketComments.Remove(comment);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}