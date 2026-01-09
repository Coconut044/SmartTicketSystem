using SmartTicket.API.DTOs.Request;
using SmartTicket.API.DTOs.Response;

namespace SmartTicket.API.Services
{
    public interface ICommentService
    {
        Task<List<CommentDto>> GetCommentsForTicketAsync(int ticketId);

        Task<CommentDto> AddCommentAsync(
            int ticketId,
            AddCommentDto dto,
            int userId
        );

        Task<CommentDto?> GetCommentByIdAsync(int commentId);

        Task<CommentDto> UpdateCommentAsync(
            int commentId,
            UpdateCommentDto dto
        );

        Task<bool> DeleteCommentAsync(int commentId);
    }
}
