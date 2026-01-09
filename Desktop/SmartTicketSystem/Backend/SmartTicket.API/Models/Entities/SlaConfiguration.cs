using System.ComponentModel.DataAnnotations;

namespace SmartTicket.API.Models.Entities
{
    public class SlaConfiguration
    {
        public int Id { get; set; }
        
        [Required]
        [MaxLength(50)]
        public string Priority { get; set; } = string.Empty;
        
        public int ResponseTimeHours { get; set; }
        
        public int ResolutionTimeHours { get; set; }
        
        public bool IsActive { get; set; } = true;
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
