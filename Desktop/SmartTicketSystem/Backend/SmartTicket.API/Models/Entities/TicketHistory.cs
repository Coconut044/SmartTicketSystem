using System.ComponentModel.DataAnnotations;

namespace SmartTicket.API.Models.Entities
{
    public class TicketHistory
    {
        public int Id { get; set; }
        
        public int TicketId { get; set; }
        public Ticket Ticket { get; set; } = null!;
        
        public int UserId { get; set; }
        public User User { get; set; } = null!;
        
        [Required]
        [MaxLength(50)]
        public string Action { get; set; } = string.Empty; // Created, StatusChanged, Assigned, etc.
        
        [MaxLength(100)]
        public string? OldValue { get; set; }
        
        [MaxLength(100)]
        public string? NewValue { get; set; }
        
        public string? Notes { get; set; }
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}