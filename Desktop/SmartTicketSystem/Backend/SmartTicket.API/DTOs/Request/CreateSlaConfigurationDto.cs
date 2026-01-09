using System.ComponentModel.DataAnnotations;

namespace SmartTicket.API.DTOs.Request
{
    public class CreateSlaConfigurationDto
    {
        [Required]
        [RegularExpression("^(Low|Medium|High|Critical)$")]
        public string Priority { get; set; } = string.Empty;

        [Required]
        [Range(1, 168)] // Max 1 week
        public int ResponseTimeHours { get; set; }

        [Required]
        [Range(1, 720)] // Max 1 month
        public int ResolutionTimeHours { get; set; }
    }
}
