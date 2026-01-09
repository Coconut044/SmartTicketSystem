namespace SmartTicket.API.DTOs.Response
{
    public class ReportByStatusDto
    {
        public Dictionary<string, int> StatusCounts { get; set; } = new();
        public Dictionary<string, double> StatusPercentages { get; set; } = new();
        public int TotalTickets { get; set; }
    }

    public class ReportByPriorityDto
    {
        public Dictionary<string, int> PriorityCounts { get; set; } = new();
        public Dictionary<string, double> PriorityPercentages { get; set; } = new();
        public int TotalTickets { get; set; }
    }

    public class ReportByCategoryDto
    {
        public Dictionary<string, int> CategoryCounts { get; set; } = new();
        public Dictionary<string, double> CategoryPercentages { get; set; } = new();
        public int TotalTickets { get; set; }
    }

    public class SlaComplianceReportDto
    {
        public int TotalTickets { get; set; }
        public int WithinSla { get; set; }
        public int BreachedSla { get; set; }
        public double ComplianceRate { get; set; }
        public List<SlaBreachDetail> Breaches { get; set; } = new();
    }

    public class SlaBreachDetail
    {
        public int TicketId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Priority { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime DueDate { get; set; }
        public double HoursOverdue { get; set; }
    }

    public class AgentWorkloadReportDto
    {
        public List<AgentWorkload> AgentWorkloads { get; set; } = new();
        public double AverageWorkload { get; set; }
    }

    public class AgentWorkload
    {
        public int AgentId { get; set; }
        public string AgentName { get; set; } = string.Empty;
        public int OpenTickets { get; set; }
        public int InProgressTickets { get; set; }
        public int ResolvedTickets { get; set; }
        public int TotalAssigned { get; set; }
        public double AverageResolutionTimeHours { get; set; }
    }

    public class ResolutionTimeReportDto
    {
        public double AverageResolutionTimeHours { get; set; }
        public double MedianResolutionTimeHours { get; set; }
        public Dictionary<string, double> ByPriority { get; set; } = new();
        public Dictionary<string, double> ByCategory { get; set; } = new();
    }

    public class TicketTrendReportDto
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public List<DailyTicketCount> DailyCounts { get; set; } = new();
        public int TotalCreated { get; set; }
        public int TotalResolved { get; set; }
    }

    public class DailyTicketCount
    {
        public DateTime Date { get; set; }
        public int Created { get; set; }
        public int Resolved { get; set; }
    }
}
