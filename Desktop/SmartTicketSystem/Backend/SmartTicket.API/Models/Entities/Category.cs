using System.ComponentModel.DataAnnotations;

namespace SmartTicket.API.Models.Entities
{
    public class Category
    {
        public int Id { get; set; }
        
        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;
        
        [MaxLength(500)]
        public string? Description { get; set; }
        
        public bool IsActive { get; set; } = true;
        
        public int? SlaHours { get; set; } // SLA in hours
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        // Navigation properties
        public ICollection<Ticket> Tickets { get; set; } = new List<Ticket>();
    }
}