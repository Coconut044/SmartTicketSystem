using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartTicket.API.DTOs.Response;
using SmartTicket.API.Services;

namespace SmartTicket.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin,SupportManager")]
    public class ReportsController : ControllerBase
    {
        private readonly IReportingService _reportingService;

        public ReportsController(IReportingService reportingService)
        {
            _reportingService = reportingService;
        }

        [HttpGet("by-status")]
        public async Task<ActionResult<ApiResponseDto<ReportByStatusDto>>> GetByStatus()
        {
            var report = await _reportingService.GetTicketsByStatusReport();
            return Ok(new ApiResponseDto<ReportByStatusDto>
            {
                Success = true,
                Message = "Report generated successfully",
                Data = report
            });
        }

        [HttpGet("by-priority")]
        public async Task<ActionResult<ApiResponseDto<ReportByPriorityDto>>> GetByPriority()
        {
            var report = await _reportingService.GetTicketsByPriorityReport();
            return Ok(new ApiResponseDto<ReportByPriorityDto>
            {
                Success = true,
                Message = "Report generated successfully",
                Data = report
            });
        }

        [HttpGet("by-category")]
        public async Task<ActionResult<ApiResponseDto<ReportByCategoryDto>>> GetByCategory()
        {
            var report = await _reportingService.GetTicketsByCategoryReport();
            return Ok(new ApiResponseDto<ReportByCategoryDto>
            {
                Success = true,
                Message = "Report generated successfully",
                Data = report
            });
        }

        [HttpGet("sla-compliance")]
        public async Task<ActionResult<ApiResponseDto<SlaComplianceReportDto>>> GetSlaCompliance()
        {
            var report = await _reportingService.GetSlaComplianceReport();
            return Ok(new ApiResponseDto<SlaComplianceReportDto>
            {
                Success = true,
                Message = "SLA compliance report generated",
                Data = report
            });
        }

        [HttpGet("agent-workload")]
        public async Task<ActionResult<ApiResponseDto<AgentWorkloadReportDto>>> GetAgentWorkload()
        {
            var report = await _reportingService.GetAgentWorkloadReport();
            return Ok(new ApiResponseDto<AgentWorkloadReportDto>
            {
                Success = true,
                Message = "Agent workload report generated",
                Data = report
            });
        }

        [HttpGet("resolution-time")]
        public async Task<ActionResult<ApiResponseDto<ResolutionTimeReportDto>>> GetResolutionTime()
        {
            var report = await _reportingService.GetResolutionTimeReport();
            return Ok(new ApiResponseDto<ResolutionTimeReportDto>
            {
                Success = true,
                Message = "Resolution time report generated",
                Data = report
            });
        }

        [HttpGet("trend")]
        public async Task<ActionResult<ApiResponseDto<TicketTrendReportDto>>> GetTrend(
            [FromQuery] DateTime? startDate,
            [FromQuery] DateTime? endDate)
        {
            var start = startDate ?? DateTime.UtcNow.AddDays(-30);
            var end = endDate ?? DateTime.UtcNow;

            var report = await _reportingService.GetTicketTrendReport(start, end);
            return Ok(new ApiResponseDto<TicketTrendReportDto>
            {
                Success = true,
                Message = "Ticket trend report generated",
                Data = report
            });
        }
    }
}