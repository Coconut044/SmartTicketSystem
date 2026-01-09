using System.ComponentModel.DataAnnotations;

namespace SmartTicket.API.Models.Entities
{
    public class Ticket
    {
        public int Id { get; set; }
        
        [Required]
        [MaxLength(200)]
        public string Title { get; set; } = string.Empty;
        
        [Required]
        public string Description { get; set; } = string.Empty;
        
        [Required]
        [MaxLength(50)]
        public string Status { get; set; } = "Created"; // Created, Assigned, InProgress, Resolved, Closed, Cancelled
        
        [Required]
        [MaxLength(50)]
        public string Priority { get; set; } = "Medium"; // Low, Medium, High, Critical
        
        public int CategoryId { get; set; }
        public Category Category { get; set; } = null!;
        
        public int CreatedById { get; set; }
        public User CreatedBy { get; set; } = null!;
        
        public int? AssignedToId { get; set; }
        public User? AssignedTo { get; set; }
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        public DateTime? UpdatedAt { get; set; }
        
        public DateTime? ResolvedAt { get; set; }
        
        public DateTime? ClosedAt { get; set; }
        
        public DateTime? DueDate { get; set; }
        
        public string? ResolutionNotes { get; set; }
        
        // Navigation properties
        public ICollection<TicketComment> Comments { get; set; } = new List<TicketComment>();
        public ICollection<TicketHistory> History { get; set; } = new List<TicketHistory>();
    }
}