namespace SmartTicket.API.DTOs.Response
{
    public class CategoryDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public bool IsActive { get; set; }
        public int? SlaHours { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}