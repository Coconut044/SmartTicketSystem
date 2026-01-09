using System.ComponentModel.DataAnnotations;

namespace SmartTicket.API.DTOs.Request
{
    public class CreateCategoryDto
    {
        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;
        
        [MaxLength(500)]
        public string? Description { get; set; }
        
        public int? SlaHours { get; set; }
    }
}