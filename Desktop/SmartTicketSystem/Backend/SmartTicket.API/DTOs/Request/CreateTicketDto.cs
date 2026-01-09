using System.ComponentModel.DataAnnotations;

namespace SmartTicket.API.DTOs.Request
{
    public class CreateTicketDto
    {
        [Required]
        [MaxLength(200)]
        public string Title { get; set; } = string.Empty;
        
        [Required]
        public string Description { get; set; } = string.Empty;
        
        [Required]
        public string Priority { get; set; } = "Medium";
        
        [Required]
        public int CategoryId { get; set; }
    }
}