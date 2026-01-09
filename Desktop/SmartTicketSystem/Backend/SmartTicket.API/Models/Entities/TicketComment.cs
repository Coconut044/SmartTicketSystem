using System.ComponentModel.DataAnnotations;

namespace SmartTicket.API.Models.Entities
{
    public class TicketComment
    {
        public int Id { get; set; }
        
        public int TicketId { get; set; }
        public Ticket Ticket { get; set; } = null!;
        
        public int UserId { get; set; }
        public User User { get; set; } = null!;
        
        [Required]
        public string Comment { get; set; } = string.Empty;
        
        public bool IsInternal { get; set; } = false;
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}