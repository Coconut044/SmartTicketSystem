namespace SmartTicket.API.DTOs.Response
{
    public class HistoryDto
    {
        public int Id { get; set; }
        public int TicketId { get; set; }
        public int UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string Action { get; set; } = string.Empty;
        public string? OldValue { get; set; }
        public string? NewValue { get; set; }
        public string? Notes { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}