namespace SmartTicket.API.DTOs.Response
{
    public class TicketDetailDto : TicketDto
    {
        public List<CommentDto> Comments { get; set; } = new();
        public List<HistoryDto> History { get; set; } = new();
    }
}