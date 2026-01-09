using Moq;
using Xunit;
using SmartTicket.API.Services;
using SmartTicket.API.Repositories;
using SmartTicket.API.Models.Entities;
using SmartTicket.API.Data;
using Microsoft.EntityFrameworkCore;

namespace SmartTicket.Tests
{
    public class TicketLifecycleServiceTests
    {
        private readonly Mock<ITicketRepository> _mockTicketRepo;
        private readonly ApplicationDbContext _context;
        private readonly TicketLifecycleService _lifecycleService;

        public TicketLifecycleServiceTests()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            
            _context = new ApplicationDbContext(options);
            _mockTicketRepo = new Mock<ITicketRepository>();
            
            _lifecycleService = new TicketLifecycleService(_mockTicketRepo.Object, _context);
        }

        [Fact]
        public async Task ValidateStatusTransition_ValidTransition_ReturnsTrue()
        {
            // Act
            var result = await _lifecycleService.ValidateStatusTransition("Created", "Assigned");

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task ValidateStatusTransition_InvalidTransition_ReturnsFalse()
        {
            // Act
            var result = await _lifecycleService.ValidateStatusTransition("Created", "Resolved");

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task ResolveTicket_ValidTicket_UpdatesStatus()
        {
            // Arrange
            var ticket = new Ticket
            {
                Id = 1,
                Title = "Test",
                Status = "InProgress",
                CreatedById = 1,
                Category = new Category { Id = 1, Name = "Test" },
                CreatedBy = new User { Id = 1, FullName = "Test User" }
            };

            _mockTicketRepo.Setup(r => r.GetByIdAsync(It.IsAny<int>()))
                .ReturnsAsync(ticket);

            _mockTicketRepo.Setup(r => r.UpdateAsync(It.IsAny<Ticket>()))
                .ReturnsAsync((Ticket t) => t);

            // Act
            var result = await _lifecycleService.ResolveTicket(1, "Issue fixed", 1);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Resolved", result.Status);
            Assert.NotNull(result.ResolvedAt);
            Assert.Equal("Issue fixed", result.ResolutionNotes);
        }

        [Fact]
        public async Task ReopenTicket_ResolvedTicket_UpdatesToInProgress()
        {
            // Arrange
            var ticket = new Ticket
            {
                Id = 1,
                Status = "Resolved",
                ResolvedAt = DateTime.UtcNow,
                CreatedById = 1,
                Category = new Category { Id = 1, Name = "Test" },
                CreatedBy = new User { Id = 1, FullName = "Test User" }
            };

            _mockTicketRepo.Setup(r => r.GetByIdAsync(It.IsAny<int>()))
                .ReturnsAsync(ticket);

            _mockTicketRepo.Setup(r => r.UpdateAsync(It.IsAny<Ticket>()))
                .ReturnsAsync((Ticket t) => t);

            // Act
            var result = await _lifecycleService.ReopenTicket(1, "Issue not fixed", 1);

            // Assert
            Assert.Equal("InProgress", result.Status);
            Assert.Null(result.ResolvedAt);
        }
    }
}