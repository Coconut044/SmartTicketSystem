namespace SmartTicket.API.DTOs.Response
{
    public class DashboardStatsDto
    {
        public int TotalTickets { get; set; }
        public int OpenTickets { get; set; }
        public int InProgressTickets { get; set; }
        public int ResolvedTickets { get; set; }
        public int ClosedTickets { get; set; }
        public int OverdueTickets { get; set; }
        public Dictionary<string, int> TicketsByPriority { get; set; } = new();
        public Dictionary<string, int> TicketsByCategory { get; set; } = new();
        public Dictionary<string, int> TicketsByStatus { get; set; } = new();
        public double AverageResolutionTimeHours { get; set; }
    }
}