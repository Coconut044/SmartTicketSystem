using System.ComponentModel.DataAnnotations;

namespace SmartTicket.API.DTOs.Request
{
    public class UpdateTicketDto
    {
        [MaxLength(200)]
        public string? Title { get; set; }
        
        public string? Description { get; set; }
        
        public string? Priority { get; set; }
        
        public int? CategoryId { get; set; }
        
        public string? Status { get; set; }
        
        public int? AssignedToId { get; set; }
        
        public string? ResolutionNotes { get; set; }
    }
}