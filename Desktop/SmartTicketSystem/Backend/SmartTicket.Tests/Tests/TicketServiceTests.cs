using Moq;
using Xunit;
using SmartTicket.API.Services;
using SmartTicket.API.Repositories;
using SmartTicket.API.Models.Entities;
using SmartTicket.API.DTOs.Request;
using SmartTicket.API.Data;
using Microsoft.EntityFrameworkCore;

namespace SmartTicket.Tests
{
    public class TicketServiceTests
    {
        private readonly Mock<ITicketRepository> _mockTicketRepo;
        private readonly Mock<IUserRepository> _mockUserRepo;
        private readonly Mock<ICategoryRepository> _mockCategoryRepo;
        private readonly ApplicationDbContext _context;
        private readonly TicketService _ticketService;

        public TicketServiceTests()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            
            _context = new ApplicationDbContext(options);
            _mockTicketRepo = new Mock<ITicketRepository>();
            _mockUserRepo = new Mock<IUserRepository>();
            _mockCategoryRepo = new Mock<ICategoryRepository>();
            
            _ticketService = new TicketService(
                _mockTicketRepo.Object,
                _mockUserRepo.Object,
                _mockCategoryRepo.Object,
                _context
            );
        }

        [Fact]
        public async Task CreateTicketAsync_ValidData_ReturnsTicketDto()
        {
            // Arrange
            var category = new Category
            {
                Id = 1,
                Name = "Test Category",
                SlaHours = 24,
                IsActive = true
            };

            _mockCategoryRepo.Setup(r => r.GetByIdAsync(It.IsAny<int>()))
                .ReturnsAsync(category);

            var ticket = new Ticket
            {
                Id = 1,
                Title = "Test Ticket",
                Description = "Test Description",
                Priority = "Medium",
                CategoryId = 1,
                Category = category,
                CreatedById = 1,
                CreatedBy = new User { Id = 1, FullName = "Test User" },
                Status = "Created",
                CreatedAt = DateTime.UtcNow
            };

            _mockTicketRepo.Setup(r => r.CreateAsync(It.IsAny<Ticket>()))
                .ReturnsAsync(ticket);

            var createDto = new CreateTicketDto
            {
                Title = "Test Ticket",
                Description = "Test Description",
                Priority = "Medium",
                CategoryId = 1
            };

            // Act
            var result = await _ticketService.CreateTicketAsync(createDto, 1);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(createDto.Title, result.Title);
            Assert.Equal("Created", result.Status);
        }

        [Fact]
        public async Task CreateTicketAsync_InvalidCategory_ThrowsInvalidOperationException()
        {
            // Arrange
            _mockCategoryRepo.Setup(r => r.GetByIdAsync(It.IsAny<int>()))
                .ReturnsAsync((Category?)null);

            var createDto = new CreateTicketDto
            {
                Title = "Test Ticket",
                Description = "Test Description",
                Priority = "Medium",
                CategoryId = 999
            };

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(
                () => _ticketService.CreateTicketAsync(createDto, 1)
            );
        }

        [Fact]
        public async Task AssignTicketAsync_ValidData_AssignsTicket()
        {
            // Arrange
            var ticket = new Ticket
            {
                Id = 1,
                Title = "Test Ticket",
                Status = "Created",
                CreatedById = 1,
                Category = new Category { Id = 1, Name = "Test" },
                CreatedBy = new User { Id = 1, FullName = "Creator" }
            };

            var agent = new User
            {
                Id = 2,
                FullName = "Agent User",
                Role = "SupportAgent"
            };

            _mockTicketRepo.Setup(r => r.GetByIdAsync(It.IsAny<int>()))
                .ReturnsAsync(ticket);
            
            _mockUserRepo.Setup(r => r.GetByIdAsync(It.IsAny<int>()))
                .ReturnsAsync(agent);

            _mockTicketRepo.Setup(r => r.UpdateAsync(It.IsAny<Ticket>()))
                .ReturnsAsync((Ticket t) => t);

            // Act
            var result = await _ticketService.AssignTicketAsync(1, 2, 1);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.AssignedToId);
            Assert.Equal("Assigned", result.Status);
        }
    }
}