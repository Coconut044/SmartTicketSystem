using Microsoft.EntityFrameworkCore;
using SmartTicket.API.Data;
using SmartTicket.API.DTOs.Request;
using SmartTicket.API.DTOs.Response;
using SmartTicket.API.Models.Entities;

namespace SmartTicket.API.Services
{
    public class CommentService : ICommentService
    {
        private readonly ApplicationDbContext _context;

        public CommentService(ApplicationDbContext context)
        {
            _context = context;
        }

        // ===================== Get all comments for a ticket =====================
        public async Task<List<CommentDto>> GetCommentsForTicketAsync(int ticketId)
        {
            return await _context.TicketComments
                .Include(tc => tc.User)
                .Where(tc => tc.TicketId == ticketId)
                .OrderBy(tc => tc.CreatedAt)
                .Select(tc => new CommentDto
                {
                    Id = tc.Id,
                    TicketId = tc.TicketId,
                    UserId = tc.UserId,
                    UserName = tc.User.FullName,
                    Comment = tc.Comment,
                    IsInternal = tc.IsInternal,
                    CreatedAt = tc.CreatedAt
                })
                .ToListAsync();
        }

        // ===================== Add comment =====================
        public async Task<CommentDto> AddCommentAsync(
            int ticketId,
            AddCommentDto dto,
            int userId
        )
        {
            var ticketComment = new TicketComment
            {
                TicketId = ticketId,
                UserId = userId,
                Comment = dto.Comment,   // assuming AddCommentDto uses Comment
                IsInternal = dto.IsInternal,
                CreatedAt = DateTime.UtcNow
            };

            _context.TicketComments.Add(ticketComment);
            await _context.SaveChangesAsync();

            var user = await _context.Users.FindAsync(userId);

            return new CommentDto
            {
                Id = ticketComment.Id,
                TicketId = ticketComment.TicketId,
                UserId = ticketComment.UserId,
                UserName = user?.FullName ?? string.Empty,
                Comment = ticketComment.Comment,
                IsInternal = ticketComment.IsInternal,
                CreatedAt = ticketComment.CreatedAt
            };
        }

        // ===================== Get comment by ID =====================
        public async Task<CommentDto?> GetCommentByIdAsync(int commentId)
        {
            return await _context.TicketComments
                .Include(tc => tc.User)
                .Where(tc => tc.Id == commentId)
                .Select(tc => new CommentDto
                {
                    Id = tc.Id,
                    TicketId = tc.TicketId,
                    UserId = tc.UserId,
                    UserName = tc.User.FullName,
                    Comment = tc.Comment,
                    IsInternal = tc.IsInternal,
                    CreatedAt = tc.CreatedAt
                })
                .FirstOrDefaultAsync();
        }

        // ===================== Update comment (FIXED FOR Text) =====================
        public async Task<CommentDto> UpdateCommentAsync(
            int commentId,
            UpdateCommentDto dto
        )
        {
            var comment = await _context.TicketComments
                .Include(tc => tc.User)
                .FirstOrDefaultAsync(tc => tc.Id == commentId);

            if (comment == null)
                throw new InvalidOperationException("Comment not found");

            // ✅ IMPORTANT FIX
            comment.Comment = dto.Text;
            comment.IsInternal = dto.IsInternal;

            await _context.SaveChangesAsync();

            return new CommentDto
            {
                Id = comment.Id,
                TicketId = comment.TicketId,
                UserId = comment.UserId,
                UserName = comment.User.FullName,
                Comment = comment.Comment,
                IsInternal = comment.IsInternal,
                CreatedAt = comment.CreatedAt
            };
        }

        // ===================== Delete comment =====================
        public async Task<bool> DeleteCommentAsync(int commentId)
        {
            var comment = await _context.TicketComments
                .FirstOrDefaultAsync(tc => tc.Id == commentId);

            if (comment == null)
                throw new InvalidOperationException("Comment not found");

            _context.TicketComments.Remove(comment);
            await _context.SaveChangesAsync();

            return true;
        }
    }
}
