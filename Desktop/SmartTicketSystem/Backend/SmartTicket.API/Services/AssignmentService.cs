using Microsoft.EntityFrameworkCore;
using SmartTicket.API.Data;
using SmartTicket.API.Models.Entities;
using SmartTicket.API.Repositories;

namespace SmartTicket.API.Services
{
    public class AssignmentService : IAssignmentService
    {
        private readonly ITicketRepository _ticketRepository;
        private readonly IUserRepository _userRepository;
        private readonly ApplicationDbContext _context;

        public AssignmentService(
            ITicketRepository ticketRepository,
            IUserRepository userRepository,
            ApplicationDbContext context)
        {
            _ticketRepository = ticketRepository;
            _userRepository = userRepository;
            _context = context;
        }

        public async Task<Ticket> AssignTicketManually(int ticketId, int agentId, int assignedBy)
        {
            var ticket = await _ticketRepository.GetByIdAsync(ticketId)
                         ?? throw new InvalidOperationException("Ticket not found");

            var agent = await _userRepository.GetByIdAsync(agentId);
            if (agent == null || !new[] { "SupportAgent", "SupportManager", "Admin" }.Contains(agent.Role))
                throw new InvalidOperationException("Invalid agent");

            ticket.AssignedToId = agentId;
            ticket.Status = "Assigned";
            ticket.UpdatedAt = DateTime.UtcNow;

            await _ticketRepository.UpdateAsync(ticket);

            _context.TicketHistories.Add(new TicketHistory
            {
                TicketId = ticketId,
                UserId = assignedBy,
                Action = "Assigned",
                NewValue = agent.FullName,
                Notes = $"Ticket manually assigned to {agent.FullName}",
                CreatedAt = DateTime.UtcNow
            });

            await _context.SaveChangesAsync();
            return ticket;
        }

        public async Task<Ticket> AssignTicketAutomatically(int ticketId)
        {
            var ticket = await _ticketRepository.GetByIdAsync(ticketId)
                         ?? throw new InvalidOperationException("Ticket not found");

            var agents = await _userRepository.GetByRoleAsync("SupportAgent");
            if (!agents.Any())
                throw new InvalidOperationException("No available agents");

            var workload = await GetAgentWorkload();

            var selectedAgent = agents
                .OrderBy(a => workload.GetValueOrDefault(a.Id, 0))
                .First();

            ticket.AssignedToId = selectedAgent.Id;
            ticket.Status = "Assigned";
            ticket.UpdatedAt = DateTime.UtcNow;

            await _ticketRepository.UpdateAsync(ticket);

            _context.TicketHistories.Add(new TicketHistory
            {
                TicketId = ticketId,
                UserId = 1, // System
                Action = "AutoAssigned",
                NewValue = selectedAgent.FullName,
                Notes = $"Ticket automatically assigned to {selectedAgent.FullName} based on workload",
                CreatedAt = DateTime.UtcNow
            });

            await _context.SaveChangesAsync();
            return ticket;
        }

        public async Task<List<Ticket>> GetUnassignedTickets()
        {
            return await _context.Tickets
                .Include(t => t.Category)
                .Include(t => t.CreatedBy)
                .Where(t => t.AssignedToId == null && t.Status == "Created")
                .OrderByDescending(t =>
                    t.Priority == "Critical" ? 4 :
                    t.Priority == "High" ? 3 :
                    t.Priority == "Medium" ? 2 : 1)
                .ThenBy(t => t.CreatedAt)
                .ToListAsync();
        }

        public async Task<Dictionary<int, int>> GetAgentWorkload()
        {
            return await _context.Tickets
                .Where(t => t.AssignedToId.HasValue &&
                            t.Status != "Closed" &&
                            t.Status != "Resolved")
                .GroupBy(t => t.AssignedToId!.Value)
                .Select(g => new { AgentId = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.AgentId, x => x.Count);
        }

        public async Task<List<Ticket>> CheckAndEscalateOverdueTickets()
        {
            var now = DateTime.UtcNow;

            var overdueTickets = await _context.Tickets
                .Include(t => t.Category)
                .Where(t => t.DueDate.HasValue &&
                            t.Status != "Resolved" &&
                            t.Status != "Closed")
                .ToListAsync();

            foreach (var ticket in overdueTickets)
            {
                var dueDate = ticket.DueDate;
                if (dueDate == null || dueDate.Value >= now)
                    continue;

                ticket.Priority = ticket.Priority switch
                {
                    "Low" => "Medium",
                    "Medium" => "High",
                    "High" => "Critical",
                    _ => ticket.Priority
                };

                ticket.UpdatedAt = DateTime.UtcNow;

                _context.TicketHistories.Add(new TicketHistory
                {
                    TicketId = ticket.Id,
                    UserId = 1, // System
                    Action = "Escalated",
                    Notes = $"Ticket escalated to {ticket.Priority} due to SLA breach",
                    CreatedAt = DateTime.UtcNow
                });

                if (ticket.AssignedToId.HasValue)
                {
                    _context.Notifications.Add(new Notification
                    {
                        UserId = ticket.AssignedToId.Value,
                        TicketId = ticket.Id,
                        Title = "SLA Breach - Ticket Escalated",
                        Message = $"Ticket #{ticket.Id} has breached SLA and been escalated to {ticket.Priority}",
                        CreatedAt = DateTime.UtcNow
                    });
                }
            }

            await _context.SaveChangesAsync();
            return overdueTickets;
        }

        public Task<bool> CheckSlaBreached(Ticket ticket)
        {
            if (!ticket.DueDate.HasValue)
                return Task.FromResult(false);

            return Task.FromResult(
                ticket.DueDate.Value < DateTime.UtcNow &&
                ticket.Status != "Resolved" &&
                ticket.Status != "Closed"
            );
        }
    }
}
