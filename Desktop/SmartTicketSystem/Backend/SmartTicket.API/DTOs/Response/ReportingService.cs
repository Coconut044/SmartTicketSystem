using Microsoft.EntityFrameworkCore;
using SmartTicket.API.Data;
using SmartTicket.API.DTOs.Response;

namespace SmartTicket.API.Services
{
    public class ReportingService : IReportingService
    {
        private readonly ApplicationDbContext _context;

        public ReportingService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<ReportByStatusDto> GetTicketsByStatusReport()
        {
            var statusCounts = await _context.Tickets
                .GroupBy(t => t.Status)
                .Select(g => new { Status = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.Status, x => x.Count);

            var total = statusCounts.Values.Sum();
            var percentages = statusCounts.ToDictionary(
                kvp => kvp.Key,
                kvp => total > 0 ? (kvp.Value * 100.0 / total) : 0
            );

            return new ReportByStatusDto
            {
                StatusCounts = statusCounts,
                StatusPercentages = percentages,
                TotalTickets = total
            };
        }

        public async Task<ReportByPriorityDto> GetTicketsByPriorityReport()
        {
            var priorityCounts = await _context.Tickets
                .GroupBy(t => t.Priority)
                .Select(g => new { Priority = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.Priority, x => x.Count);

            var total = priorityCounts.Values.Sum();
            var percentages = priorityCounts.ToDictionary(
                kvp => kvp.Key,
                kvp => total > 0 ? (kvp.Value * 100.0 / total) : 0
            );

            return new ReportByPriorityDto
            {
                PriorityCounts = priorityCounts,
                PriorityPercentages = percentages,
                TotalTickets = total
            };
        }

        public async Task<ReportByCategoryDto> GetTicketsByCategoryReport()
        {
            var categoryCounts = await _context.Tickets
                .Include(t => t.Category)
                .GroupBy(t => t.Category.Name)
                .Select(g => new { Category = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.Category, x => x.Count);

            var total = categoryCounts.Values.Sum();
            var percentages = categoryCounts.ToDictionary(
                kvp => kvp.Key,
                kvp => total > 0 ? (kvp.Value * 100.0 / total) : 0
            );

            return new ReportByCategoryDto
            {
                CategoryCounts = categoryCounts,
                CategoryPercentages = percentages,
                TotalTickets = total
            };
        }

        public async Task<SlaComplianceReportDto> GetSlaComplianceReport()
        {
            var now = DateTime.UtcNow;
            
            var allTickets = await _context.Tickets
                .Include(t => t.Category)
                .Where(t => t.DueDate.HasValue)
                .ToListAsync();

            var withinSla = allTickets.Count(t => 
                t.Status == "Resolved" && t.ResolvedAt.HasValue && t.ResolvedAt.Value <= t.DueDate!.Value ||
                t.Status == "Closed" && t.ClosedAt.HasValue && t.ClosedAt.Value <= t.DueDate!.Value ||
                (t.Status != "Resolved" && t.Status != "Closed" && now <= t.DueDate!.Value)
            );

            var breachedTickets = allTickets.Where(t => 
                (t.Status == "Resolved" && t.ResolvedAt.HasValue && t.ResolvedAt.Value > t.DueDate!.Value) ||
                (t.Status == "Closed" && t.ClosedAt.HasValue && t.ClosedAt.Value > t.DueDate!.Value) ||
                (t.Status != "Resolved" && t.Status != "Closed" && now > t.DueDate!.Value)
            ).Select(t => new SlaBreachDetail
            {
                TicketId = t.Id,
                Title = t.Title,
                Priority = t.Priority,
                CreatedAt = t.CreatedAt,
                DueDate = t.DueDate!.Value,
                HoursOverdue = (now - t.DueDate!.Value).TotalHours
            }).ToList();

            var total = allTickets.Count;
            var breached = breachedTickets.Count;

            return new SlaComplianceReportDto
            {
                TotalTickets = total,
                WithinSla = withinSla,
                BreachedSla = breached,
                ComplianceRate = total > 0 ? (withinSla * 100.0 / total) : 0,
                Breaches = breachedTickets
            };
        }

        public async Task<AgentWorkloadReportDto> GetAgentWorkloadReport()
        {
            var agents = await _context.Users
                .Where(u => u.Role == "SupportAgent" && u.IsActive)
                .ToListAsync();

            var workloads = new List<AgentWorkload>();

            foreach (var agent in agents)
            {
                var tickets = await _context.Tickets
                    .Where(t => t.AssignedToId == agent.Id)
                    .ToListAsync();

                var resolvedTickets = tickets.Where(t => t.Status == "Resolved" && t.ResolvedAt.HasValue).ToList();
                var avgResolutionTime = resolvedTickets.Any()
                    ? resolvedTickets.Average(t => (t.ResolvedAt!.Value - t.CreatedAt).TotalHours)
                    : 0;

                workloads.Add(new AgentWorkload
                {
                    AgentId = agent.Id,
                    AgentName = agent.FullName,
                    OpenTickets = tickets.Count(t => t.Status == "Created" || t.Status == "Assigned"),
                    InProgressTickets = tickets.Count(t => t.Status == "InProgress"),
                    ResolvedTickets = tickets.Count(t => t.Status == "Resolved"),
                    TotalAssigned = tickets.Count,
                    AverageResolutionTimeHours = Math.Round(avgResolutionTime, 2)
                });
            }

            return new AgentWorkloadReportDto
            {
                AgentWorkloads = workloads,
                AverageWorkload = workloads.Any() ? workloads.Average(w => w.TotalAssigned) : 0
            };
        }

        public async Task<ResolutionTimeReportDto> GetResolutionTimeReport()
        {
            var resolvedTickets = await _context.Tickets
                .Include(t => t.Category)
                .Where(t => t.ResolvedAt.HasValue)
                .ToListAsync();

            if (!resolvedTickets.Any())
            {
                return new ResolutionTimeReportDto();
            }

            var resolutionTimes = resolvedTickets
                .Select(t => (t.ResolvedAt!.Value - t.CreatedAt).TotalHours)
                .OrderBy(x => x)
                .ToList();

            var byPriority = resolvedTickets
                .GroupBy(t => t.Priority)
                .ToDictionary(
                    g => g.Key,
                    g => g.Average(t => (t.ResolvedAt!.Value - t.CreatedAt).TotalHours)
                );

            var byCategory = resolvedTickets
                .GroupBy(t => t.Category.Name)
                .ToDictionary(
                    g => g.Key,
                    g => g.Average(t => (t.ResolvedAt!.Value - t.CreatedAt).TotalHours)
                );

            return new ResolutionTimeReportDto
            {
                AverageResolutionTimeHours = Math.Round(resolutionTimes.Average(), 2),
                MedianResolutionTimeHours = Math.Round(resolutionTimes[resolutionTimes.Count / 2], 2),
                ByPriority = byPriority.ToDictionary(kvp => kvp.Key, kvp => Math.Round(kvp.Value, 2)),
                ByCategory = byCategory.ToDictionary(kvp => kvp.Key, kvp => Math.Round(kvp.Value, 2))
            };
        }

        public async Task<TicketTrendReportDto> GetTicketTrendReport(DateTime startDate, DateTime endDate)
        {
            var tickets = await _context.Tickets
                .Where(t => t.CreatedAt >= startDate && t.CreatedAt <= endDate)
                .ToListAsync();

            var dailyCounts = tickets
                .GroupBy(t => t.CreatedAt.Date)
                .Select(g => new DailyTicketCount
                {
                    Date = g.Key,
                    Created = g.Count(),
                    Resolved = g.Count(t => t.ResolvedAt.HasValue && t.ResolvedAt.Value.Date == g.Key)
                })
                .OrderBy(d => d.Date)
                .ToList();

            return new TicketTrendReportDto
            {
                StartDate = startDate,
                EndDate = endDate,
                DailyCounts = dailyCounts,
                TotalCreated = tickets.Count,
                TotalResolved = tickets.Count(t => t.ResolvedAt.HasValue)
            };
        }
    }
}