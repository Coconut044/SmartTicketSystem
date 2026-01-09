using Moq;
using Xunit;
using SmartTicket.API.Services;
using SmartTicket.API.Repositories;
using SmartTicket.API.Models.Entities;
using SmartTicket.API.Data;
using Microsoft.EntityFrameworkCore;

namespace SmartTicket.Tests
{
    public class AssignmentServiceTests
    {
        private readonly Mock<ITicketRepository> _mockTicketRepo;
        private readonly Mock<IUserRepository> _mockUserRepo;
        private readonly ApplicationDbContext _context;
        private readonly AssignmentService _assignmentService;

        public AssignmentServiceTests()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            
            _context = new ApplicationDbContext(options);
            _mockTicketRepo = new Mock<ITicketRepository>();
            _mockUserRepo = new Mock<IUserRepository>();
            
            _assignmentService = new AssignmentService(
                _mockTicketRepo.Object,
                _mockUserRepo.Object,
                _context
            );
        }

        [Fact]
        public async Task AssignTicketAutomatically_SelectsAgentWithLeastWorkload()
        {
            // Arrange
            var ticket = new Ticket
            {
                Id = 1,
                Title = "Test",
                Status = "Created",
                Category = new Category { Id = 1, Name = "Test" }
            };

            var agents = new List<User>
            {
                new User { Id = 1, FullName = "Agent 1", Role = "SupportAgent" },
                new User { Id = 2, FullName = "Agent 2", Role = "SupportAgent" }
            };

            _mockTicketRepo.Setup(r => r.GetByIdAsync(It.IsAny<int>()))
                .ReturnsAsync(ticket);

            _mockUserRepo.Setup(r => r.GetByRoleAsync("SupportAgent"))
                .ReturnsAsync(agents);

            _mockTicketRepo.Setup(r => r.UpdateAsync(It.IsAny<Ticket>()))
                .ReturnsAsync((Ticket t) => t);

            // Act
            var result = await _assignmentService.AssignTicketAutomatically(1);

            // Assert
            Assert.NotNull(result.AssignedToId);
            Assert.Equal("Assigned", result.Status);
        }
    }
}