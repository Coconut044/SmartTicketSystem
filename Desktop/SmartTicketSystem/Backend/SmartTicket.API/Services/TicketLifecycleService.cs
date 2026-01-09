using SmartTicket.API.Data;
using SmartTicket.API.Models.Entities;
using SmartTicket.API.Repositories;

namespace SmartTicket.API.Services
{
    public class TicketLifecycleService : ITicketLifecycleService
    {
        private readonly ITicketRepository _ticketRepository;
        private readonly ApplicationDbContext _context;

        // Define valid status transitions
        private readonly Dictionary<string, List<string>> _statusTransitions = new()
        {
            { "Created", new List<string> { "Assigned", "Cancelled" } },
            { "Assigned", new List<string> { "InProgress", "Cancelled" } },
            { "InProgress", new List<string> { "Resolved", "Assigned", "Cancelled" } },
            { "Resolved", new List<string> { "Closed", "InProgress" } }, // Can reopen
            { "Closed", new List<string> { "InProgress" } }, // Can reopen
            { "Cancelled", new List<string> { } } // Terminal state
        };

        public TicketLifecycleService(ITicketRepository ticketRepository, ApplicationDbContext context)
        {
            _ticketRepository = ticketRepository;
            _context = context;
        }

        public async Task<Ticket> MoveToInProgress(int ticketId, int userId)
        {
            var ticket = await _ticketRepository.GetByIdAsync(ticketId);
            if (ticket == null)
                throw new InvalidOperationException("Ticket not found");

            if (!await ValidateStatusTransition(ticket.Status, "InProgress"))
                throw new InvalidOperationException($"Cannot move from {ticket.Status} to InProgress");

            var oldStatus = ticket.Status;
            ticket.Status = "InProgress";
            ticket.UpdatedAt = DateTime.UtcNow;

            await _ticketRepository.UpdateAsync(ticket);
            await CreateStatusChangeHistory(ticketId, userId, oldStatus, "InProgress", "Ticket moved to In Progress");

            // Create notification
            await CreateNotification(ticket.CreatedById, ticketId, 
                "Ticket Status Updated", 
                $"Your ticket #{ticketId} is now being worked on");

            return ticket;
        }

        public async Task<Ticket> ResolveTicket(int ticketId, string resolutionNotes, int userId)
        {
            var ticket = await _ticketRepository.GetByIdAsync(ticketId);
            if (ticket == null)
                throw new InvalidOperationException("Ticket not found");

            if (string.IsNullOrWhiteSpace(resolutionNotes))
                throw new InvalidOperationException("Resolution notes are required");

            if (!await ValidateStatusTransition(ticket.Status, "Resolved"))
                throw new InvalidOperationException($"Cannot resolve ticket from {ticket.Status} status");

            var oldStatus = ticket.Status;
            ticket.Status = "Resolved";
            ticket.ResolutionNotes = resolutionNotes;
            ticket.ResolvedAt = DateTime.UtcNow;
            ticket.UpdatedAt = DateTime.UtcNow;

            await _ticketRepository.UpdateAsync(ticket);
            await CreateStatusChangeHistory(ticketId, userId, oldStatus, "Resolved", $"Ticket resolved: {resolutionNotes}");

            // Create notification
            await CreateNotification(ticket.CreatedById, ticketId, 
                "Ticket Resolved", 
                $"Your ticket #{ticketId} has been resolved");

            return ticket;
        }

        public async Task<Ticket> CloseTicket(int ticketId, int userId)
        {
            var ticket = await _ticketRepository.GetByIdAsync(ticketId);
            if (ticket == null)
                throw new InvalidOperationException("Ticket not found");

            if (!await ValidateStatusTransition(ticket.Status, "Closed"))
                throw new InvalidOperationException($"Cannot close ticket from {ticket.Status} status");

            var oldStatus = ticket.Status;
            ticket.Status = "Closed";
            ticket.ClosedAt = DateTime.UtcNow;
            ticket.UpdatedAt = DateTime.UtcNow;

            await _ticketRepository.UpdateAsync(ticket);
            await CreateStatusChangeHistory(ticketId, userId, oldStatus, "Closed", "Ticket closed");

            // Create notification
            await CreateNotification(ticket.CreatedById, ticketId, 
                "Ticket Closed", 
                $"Your ticket #{ticketId} has been closed");

            return ticket;
        }

        public async Task<Ticket> ReopenTicket(int ticketId, string reason, int userId)
        {
            var ticket = await _ticketRepository.GetByIdAsync(ticketId);
            if (ticket == null)
                throw new InvalidOperationException("Ticket not found");

            if (ticket.Status != "Resolved" && ticket.Status != "Closed")
                throw new InvalidOperationException("Only resolved or closed tickets can be reopened");

            if (string.IsNullOrWhiteSpace(reason))
                throw new InvalidOperationException("Reason for reopening is required");

            var oldStatus = ticket.Status;
            ticket.Status = "InProgress";
            ticket.UpdatedAt = DateTime.UtcNow;
            ticket.ResolvedAt = null;
            ticket.ClosedAt = null;

            await _ticketRepository.UpdateAsync(ticket);
            await CreateStatusChangeHistory(ticketId, userId, oldStatus, "InProgress", $"Ticket reopened: {reason}");

            // Create notification for assigned agent
            if (ticket.AssignedToId.HasValue)
            {
                await CreateNotification(ticket.AssignedToId.Value, ticketId, 
                    "Ticket Reopened", 
                    $"Ticket #{ticketId} has been reopened: {reason}");
            }

            return ticket;
        }

        public async Task<Ticket> CancelTicket(int ticketId, string reason, int userId)
        {
            var ticket = await _ticketRepository.GetByIdAsync(ticketId);
            if (ticket == null)
                throw new InvalidOperationException("Ticket not found");

            if (ticket.Status == "Cancelled")
                throw new InvalidOperationException("Ticket is already cancelled");

            if (ticket.Status == "Closed")
                throw new InvalidOperationException("Cannot cancel a closed ticket");

            if (string.IsNullOrWhiteSpace(reason))
                throw new InvalidOperationException("Reason for cancellation is required");

            var oldStatus = ticket.Status;
            ticket.Status = "Cancelled";
            ticket.UpdatedAt = DateTime.UtcNow;

            await _ticketRepository.UpdateAsync(ticket);
            await CreateStatusChangeHistory(ticketId, userId, oldStatus, "Cancelled", $"Ticket cancelled: {reason}");

            return ticket;
        }

        public Task<bool> ValidateStatusTransition(string currentStatus, string newStatus)
        {
            if (_statusTransitions.ContainsKey(currentStatus))
            {
                return Task.FromResult(_statusTransitions[currentStatus].Contains(newStatus));
            }
            return Task.FromResult(false);
        }

        public Task<List<string>> GetAllowedNextStatuses(string currentStatus)
        {
            if (_statusTransitions.ContainsKey(currentStatus))
            {
                return Task.FromResult(_statusTransitions[currentStatus]);
            }
            return Task.FromResult(new List<string>());
        }

        private async Task CreateStatusChangeHistory(int ticketId, int userId, string oldStatus, string newStatus, string notes)
        {
            var history = new TicketHistory
            {
                TicketId = ticketId,
                UserId = userId,
                Action = "StatusChanged",
                OldValue = oldStatus,
                NewValue = newStatus,
                Notes = notes,
                CreatedAt = DateTime.UtcNow
            };
            _context.TicketHistories.Add(history);
            await _context.SaveChangesAsync();
        }

        private async Task CreateNotification(int userId, int ticketId, string title, string message)
        {
            var notification = new Notification
            {
                UserId = userId,
                TicketId = ticketId,
                Title = title,
                Message = message,
                IsRead = false,
                CreatedAt = DateTime.UtcNow
            };
            _context.Notifications.Add(notification);
            await _context.SaveChangesAsync();
        }
    }
}