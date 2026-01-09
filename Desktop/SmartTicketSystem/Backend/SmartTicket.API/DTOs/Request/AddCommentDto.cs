using System.ComponentModel.DataAnnotations;

namespace SmartTicket.API.DTOs.Request
{
    public class AddCommentDto
    {
        [Required]
        public string Comment { get; set; } = string.Empty;
        
        public bool IsInternal { get; set; } = false;
    }
}