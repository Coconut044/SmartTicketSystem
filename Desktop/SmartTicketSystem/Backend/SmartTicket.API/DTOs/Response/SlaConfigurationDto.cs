namespace SmartTicket.API.DTOs.Response
{
    public class SlaConfigurationDto
    {
        public int Id { get; set; }
        public string Priority { get; set; } = string.Empty;
        public int ResponseTimeHours { get; set; }
        public int ResolutionTimeHours { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}