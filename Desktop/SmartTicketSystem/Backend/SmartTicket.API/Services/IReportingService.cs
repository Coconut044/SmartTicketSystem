using SmartTicket.API.DTOs.Response;

namespace SmartTicket.API.Services
{
    public interface IReportingService
    {
        Task<ReportByStatusDto> GetTicketsByStatusReport();
        Task<ReportByPriorityDto> GetTicketsByPriorityReport();
        Task<ReportByCategoryDto> GetTicketsByCategoryReport();
        Task<SlaComplianceReportDto> GetSlaComplianceReport();
        Task<AgentWorkloadReportDto> GetAgentWorkloadReport();
        Task<ResolutionTimeReportDto> GetResolutionTimeReport();
        Task<TicketTrendReportDto> GetTicketTrendReport(DateTime startDate, DateTime endDate);
    }
}