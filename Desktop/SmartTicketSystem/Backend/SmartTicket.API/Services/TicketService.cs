using SmartTicket.API.Data;
using SmartTicket.API.DTOs.Request;
using SmartTicket.API.DTOs.Response;
using SmartTicket.API.Models.Entities;
using SmartTicket.API.Repositories;

namespace SmartTicket.API.Services
{
    public class TicketService : ITicketService
    {
        private readonly ITicketRepository _ticketRepository;
        private readonly IUserRepository _userRepository;
        private readonly ICategoryRepository _categoryRepository;
        private readonly ApplicationDbContext _context;
        private readonly ISlaService _slaService;
        private readonly INotificationService _notificationService;

        public TicketService(
            ITicketRepository ticketRepository,
            IUserRepository userRepository,
            ICategoryRepository categoryRepository,
            ApplicationDbContext context,
            ISlaService slaService,
            INotificationService notificationService)
        {
            _ticketRepository = ticketRepository;
            _userRepository = userRepository;
            _categoryRepository = categoryRepository;
            _context = context;
            _slaService = slaService;
            _notificationService = notificationService;
        }

        public async Task<PagedResultDto<TicketDto>> GetAllTicketsAsync(
            int pageNumber,
            int pageSize,
            string? status,
            string? priority,
            int? categoryId,
            int? assignedToId,
            int? createdById)
        {
            var tickets = await _ticketRepository.GetAllAsync(
                pageNumber, pageSize, status, priority,
                categoryId, assignedToId, createdById);

            var totalCount = await _ticketRepository.GetTotalCountAsync(
                status, priority, categoryId, assignedToId, createdById);

            var ticketDtos = tickets.Select(MapToDto).ToList();

            return new PagedResultDto<TicketDto>
            {
                Items = ticketDtos,
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize),
                HasPrevious = pageNumber > 1,
                HasNext = pageNumber * pageSize < totalCount
            };
        }

        public async Task<TicketDetailDto?> GetTicketByIdAsync(int id)
        {
            var ticket = await _ticketRepository.GetByIdWithDetailsAsync(id);
            return ticket == null ? null : MapToDetailDto(ticket);
        }

        public async Task<TicketDto> CreateTicketAsync(CreateTicketDto dto, int userId)
        {
            var category = await _categoryRepository.GetByIdAsync(dto.CategoryId);
            if (category == null)
                throw new InvalidOperationException("Category not found");

            var ticket = new Ticket
            {
                Title = dto.Title,
                Description = dto.Description,
                Priority = dto.Priority,
                CategoryId = dto.CategoryId,
                CreatedById = userId,
                Status = "Created",
                CreatedAt = DateTime.UtcNow
            };

            if (category.SlaHours.HasValue)
            {
                ticket.DueDate = DateTime.UtcNow.AddHours(category.SlaHours.Value);
            }
            else
            {
                var slaDueDate = await _slaService.CalculateDueDateAsync(
                    dto.Priority,
                    DateTime.UtcNow);

                ticket.DueDate = slaDueDate ?? DateTime.UtcNow.AddHours(24);
            }

            var createdTicket = await _ticketRepository.CreateAsync(ticket);

            await CreateHistoryAsync(
                createdTicket.Id,
                userId,
                "Created",
                null,
                null,
                "Ticket created");

            await _notificationService.NotifyAsync(
                createdTicket.CreatedById,
                $"Your ticket #{createdTicket.Id} has been created");

            return MapToDto(createdTicket);
        }

        public async Task<TicketDto> UpdateTicketAsync(
            int id,
            UpdateTicketDto dto,
            int userId)
        {
            var ticket = await _ticketRepository.GetByIdAsync(id)
                ?? throw new InvalidOperationException("Ticket not found");

            var changes = new List<(string field, string? oldValue, string? newValue)>();

            if (!string.IsNullOrEmpty(dto.Title) && dto.Title != ticket.Title)
            {
                changes.Add(("Title", ticket.Title, dto.Title));
                ticket.Title = dto.Title;
            }

            if (!string.IsNullOrEmpty(dto.Description) && dto.Description != ticket.Description)
            {
                changes.Add(("Description", null, null));
                ticket.Description = dto.Description;
            }

            if (!string.IsNullOrEmpty(dto.Priority) && dto.Priority != ticket.Priority)
            {
                changes.Add(("Priority", ticket.Priority, dto.Priority));
                ticket.Priority = dto.Priority;
            }

            if (dto.CategoryId.HasValue && dto.CategoryId.Value != ticket.CategoryId)
            {
                changes.Add(("Category",
                    ticket.CategoryId.ToString(),
                    dto.CategoryId.Value.ToString()));
                ticket.CategoryId = dto.CategoryId.Value;
            }

            if (!string.IsNullOrEmpty(dto.Status) && dto.Status != ticket.Status)
            {
                changes.Add(("Status", ticket.Status, dto.Status));
                ticket.Status = dto.Status;

                if (dto.Status == "Resolved")
                    ticket.ResolvedAt = DateTime.UtcNow;
                else if (dto.Status == "Closed")
                    ticket.ClosedAt = DateTime.UtcNow;
            }

            if (dto.AssignedToId.HasValue && dto.AssignedToId != ticket.AssignedToId)
            {
                changes.Add(("AssignedTo",
                    ticket.AssignedToId?.ToString(),
                    dto.AssignedToId.Value.ToString()));
                ticket.AssignedToId = dto.AssignedToId;
            }

            if (!string.IsNullOrEmpty(dto.ResolutionNotes))
            {
                ticket.ResolutionNotes = dto.ResolutionNotes;
            }

            var updatedTicket = await _ticketRepository.UpdateAsync(ticket);

            foreach (var change in changes)
            {
                await CreateHistoryAsync(
                    id,
                    userId,
                    $"{change.field}Changed",
                    change.oldValue,
                    change.newValue,
                    null);
            }

            return MapToDto(updatedTicket);
        }

        public async Task<bool> DeleteTicketAsync(int id)
        {
            return await _ticketRepository.DeleteAsync(id);
        }

        public async Task<TicketDto> AssignTicketAsync(
            int ticketId,
            int assignedToId,
            int userId)
        {
            var ticket = await _ticketRepository.GetByIdAsync(ticketId)
                ?? throw new InvalidOperationException("Ticket not found");

            var user = await _userRepository.GetByIdAsync(assignedToId)
                ?? throw new InvalidOperationException("User not found");

            var oldAssignedToId = ticket.AssignedToId;
            ticket.AssignedToId = assignedToId;

            if (ticket.Status == "Created")
                ticket.Status = "Assigned";

            var updatedTicket = await _ticketRepository.UpdateAsync(ticket);

            await CreateHistoryAsync(
                ticketId,
                userId,
                "Assigned",
                oldAssignedToId?.ToString(),
                assignedToId.ToString(),
                $"Ticket assigned to {user.FullName}");

            return MapToDto(updatedTicket);
        }

        public async Task<TicketDto> UpdateTicketStatusAsync(
            int ticketId,
            string status,
            int userId)
        {
            var ticket = await _ticketRepository.GetByIdAsync(ticketId)
                ?? throw new InvalidOperationException("Ticket not found");

            var oldStatus = ticket.Status;
            ticket.Status = status;

            if (status == "Resolved")
                ticket.ResolvedAt = DateTime.UtcNow;
            else if (status == "Closed")
                ticket.ClosedAt = DateTime.UtcNow;

            var updatedTicket = await _ticketRepository.UpdateAsync(ticket);

            await CreateHistoryAsync(
                ticketId,
                userId,
                "StatusChanged",
                oldStatus,
                status,
                $"Status changed from {oldStatus} to {status}");

            await _notificationService.NotifyAsync(
                ticket.CreatedById,
                $"Ticket #{ticket.Id} status changed from {oldStatus} to {status}");

            return MapToDto(updatedTicket);
        }

        public async Task<TicketDto> ReopenTicketAsync(int ticketId, int userId)
        {
            var ticket = await _ticketRepository.GetByIdAsync(ticketId)
                ?? throw new InvalidOperationException("Ticket not found");

            if (ticket.Status != "Closed" && ticket.Status != "Resolved")
                throw new InvalidOperationException("Only closed or resolved tickets can be reopened");

            var oldStatus = ticket.Status;
            ticket.Status = "Reopened";
            ticket.ResolvedAt = null;
            ticket.ClosedAt = null;

            var updatedTicket = await _ticketRepository.UpdateAsync(ticket);

            await CreateHistoryAsync(
                ticketId,
                userId,
                "Reopened",
                oldStatus,
                "Reopened",
                "Ticket reopened");

            await _notificationService.NotifyAsync(
                ticket.CreatedById,
                $"Ticket #{ticket.Id} has been reopened");

            return MapToDto(updatedTicket);
        }

        public async Task<TicketDto> CancelTicketAsync(int ticketId, int userId)
        {
            var ticket = await _ticketRepository.GetByIdAsync(ticketId)
                ?? throw new InvalidOperationException("Ticket not found");

            if (ticket.Status == "Closed")
                throw new InvalidOperationException("Closed tickets cannot be cancelled");

            var oldStatus = ticket.Status;
            ticket.Status = "Cancelled";

            var updatedTicket = await _ticketRepository.UpdateAsync(ticket);

            await CreateHistoryAsync(
                ticketId,
                userId,
                "Cancelled",
                oldStatus,
                "Cancelled",
                "Ticket cancelled");

            await _notificationService.NotifyAsync(
                ticket.CreatedById,
                $"Ticket #{ticket.Id} has been cancelled");

            return MapToDto(updatedTicket);
        }

        public async Task<CommentDto> AddCommentAsync(
            int ticketId,
            AddCommentDto dto,
            int userId)
        {
            var ticket = await _ticketRepository.GetByIdAsync(ticketId)
                ?? throw new InvalidOperationException("Ticket not found");

            var comment = new TicketComment
            {
                TicketId = ticketId,
                UserId = userId,
                Comment = dto.Comment,
                IsInternal = dto.IsInternal,
                CreatedAt = DateTime.UtcNow
            };

            _context.TicketComments.Add(comment);
            await _context.SaveChangesAsync();

            var user = await _userRepository.GetByIdAsync(userId);

            return new CommentDto
            {
                Id = comment.Id,
                TicketId = comment.TicketId,
                UserId = comment.UserId,
                UserName = user?.FullName ?? "Unknown",
                Comment = comment.Comment,
                IsInternal = comment.IsInternal,
                CreatedAt = comment.CreatedAt
            };
        }

        public async Task<DashboardStatsDto> GetDashboardStatsAsync()
        {
            var statusCounts = await _ticketRepository.GetTicketCountByStatusAsync();
            var priorityCounts = await _ticketRepository.GetTicketCountByPriorityAsync();
            var categoryCounts = await _ticketRepository.GetTicketCountByCategoryAsync();
            var avgResolutionTime = await _ticketRepository.GetAverageResolutionTimeAsync();
            var overdueTickets = await _ticketRepository.GetOverdueTicketsAsync();

            return new DashboardStatsDto
            {
                TotalTickets = statusCounts.Values.Sum(),
                OpenTickets = statusCounts.GetValueOrDefault("Created", 0)
                             + statusCounts.GetValueOrDefault("Assigned", 0),
                InProgressTickets = statusCounts.GetValueOrDefault("InProgress", 0),
                ResolvedTickets = statusCounts.GetValueOrDefault("Resolved", 0),
                ClosedTickets = statusCounts.GetValueOrDefault("Closed", 0),
                OverdueTickets = overdueTickets.Count(),
                TicketsByPriority = priorityCounts,
                TicketsByCategory = categoryCounts,
                TicketsByStatus = statusCounts,
                AverageResolutionTimeHours = Math.Round(avgResolutionTime, 2)
            };
        }

        private async Task CreateHistoryAsync(
            int ticketId,
            int userId,
            string action,
            string? oldValue,
            string? newValue,
            string? notes)
        {
            var history = new TicketHistory
            {
                TicketId = ticketId,
                UserId = userId,
                Action = action,
                OldValue = oldValue,
                NewValue = newValue,
                Notes = notes,
                CreatedAt = DateTime.UtcNow
            };

            _context.TicketHistories.Add(history);
            await _context.SaveChangesAsync();
        }

        private TicketDto MapToDto(Ticket ticket)
        {
            var isOverdue = ticket.DueDate.HasValue &&
                            ticket.DueDate < DateTime.UtcNow &&
                            ticket.Status != "Resolved" &&
                            ticket.Status != "Closed";

            return new TicketDto
            {
                Id = ticket.Id,
                Title = ticket.Title,
                Description = ticket.Description,
                Status = ticket.Status,
                Priority = ticket.Priority,
                CategoryId = ticket.CategoryId,
                CategoryName = ticket.Category?.Name ?? "Unknown",
                CreatedById = ticket.CreatedById,
                CreatedByName = ticket.CreatedBy?.FullName ?? "Unknown",
                AssignedToId = ticket.AssignedToId,
                AssignedToName = ticket.AssignedTo?.FullName,
                CreatedAt = ticket.CreatedAt,
                UpdatedAt = ticket.UpdatedAt,
                ResolvedAt = ticket.ResolvedAt,
                ClosedAt = ticket.ClosedAt,
                DueDate = ticket.DueDate,
                ResolutionNotes = ticket.ResolutionNotes,
                CommentCount = ticket.Comments?.Count ?? 0,
                IsOverdue = isOverdue
            };
        }

        private TicketDetailDto MapToDetailDto(Ticket ticket)
        {
            return new TicketDetailDto
            {
                Id = ticket.Id,
                Title = ticket.Title,
                Description = ticket.Description,
                Status = ticket.Status,
                Priority = ticket.Priority,
                CategoryId = ticket.CategoryId,
                CategoryName = ticket.Category?.Name ?? "Unknown",
                CreatedById = ticket.CreatedById,
                CreatedByName = ticket.CreatedBy?.FullName ?? "Unknown",
                AssignedToId = ticket.AssignedToId,
                AssignedToName = ticket.AssignedTo?.FullName,
                CreatedAt = ticket.CreatedAt,
                UpdatedAt = ticket.UpdatedAt,
                ResolvedAt = ticket.ResolvedAt,
                ClosedAt = ticket.ClosedAt,
                DueDate = ticket.DueDate,
                ResolutionNotes = ticket.ResolutionNotes,
                CommentCount = ticket.Comments?.Count ?? 0,
                IsOverdue = ticket.DueDate.HasValue &&
                            ticket.DueDate < DateTime.UtcNow &&
                            ticket.Status != "Resolved" &&
                            ticket.Status != "Closed",
                Comments = ticket.Comments?.Select(c => new CommentDto
                {
                    Id = c.Id,
                    TicketId = c.TicketId,
                    UserId = c.UserId,
                    UserName = c.User?.FullName ?? "Unknown",
                    Comment = c.Comment,
                    IsInternal = c.IsInternal,
                    CreatedAt = c.CreatedAt
                }).ToList() ?? new(),
                History = ticket.History?.Select(h => new HistoryDto
                {
                    Id = h.Id,
                    TicketId = h.TicketId,
                    UserId = h.UserId,
                    UserName = h.User?.FullName ?? "Unknown",
                    Action = h.Action,
                    OldValue = h.OldValue,
                    NewValue = h.NewValue,
                    Notes = h.Notes,
                    CreatedAt = h.CreatedAt
                }).ToList() ?? new()
            };
        }
    }
}
