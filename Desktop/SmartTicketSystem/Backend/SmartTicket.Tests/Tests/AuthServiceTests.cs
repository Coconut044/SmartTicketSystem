using Moq;
using Xunit;
using SmartTicket.API.Services;
using SmartTicket.API.Repositories;
using SmartTicket.API.Models.Entities;
using SmartTicket.API.DTOs.Request;
using Microsoft.Extensions.Configuration;
using Microsoft.EntityFrameworkCore;

namespace SmartTicket.Tests
{
    public class AuthServiceTests
    {
        private readonly Mock<IUserRepository> _mockUserRepo;
        private readonly Mock<IConfiguration> _mockConfig;
        private readonly AuthService _authService;

        public AuthServiceTests()
        {
            _mockUserRepo = new Mock<IUserRepository>();
            _mockConfig = new Mock<IConfiguration>();
            
            _mockConfig.Setup(c => c["Jwt:Key"]).Returns("ThisIsAVerySecureSecretKeyForJWTTokenGeneration12345!");
            _mockConfig.Setup(c => c["Jwt:Issuer"]).Returns("SmartTicketAPI");
            _mockConfig.Setup(c => c["Jwt:Audience"]).Returns("SmartTicketClient");
            
            _authService = new AuthService(_mockUserRepo.Object, _mockConfig.Object);
        }

        [Fact]
        public async Task LoginAsync_ValidCredentials_ReturnsAuthResponse()
        {
            // Arrange
            var user = new User
            {
                Id = 1,
                Email = "test@test.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("password123"),
                Role = "EndUser",
                FullName = "Test User",
                IsActive = true
            };

            _mockUserRepo.Setup(r => r.GetByEmailAsync(It.IsAny<string>()))
                .ReturnsAsync(user);

            var loginRequest = new LoginRequestDto
            {
                Email = "test@test.com",
                Password = "password123"
            };

            // Act
            var result = await _authService.LoginAsync(loginRequest);

            // Assert
            Assert.NotNull(result);
            Assert.NotNull(result.Token);
            Assert.Equal(user.Email, result.User.Email);
        }

        [Fact]
        public async Task LoginAsync_InvalidPassword_ThrowsUnauthorizedAccessException()
        {
            // Arrange
            var user = new User
            {
                Id = 1,
                Email = "test@test.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("password123"),
                Role = "EndUser",
                IsActive = true
            };

            _mockUserRepo.Setup(r => r.GetByEmailAsync(It.IsAny<string>()))
                .ReturnsAsync(user);

            var loginRequest = new LoginRequestDto
            {
                Email = "test@test.com",
                Password = "wrongpassword"
            };

            // Act & Assert
            await Assert.ThrowsAsync<UnauthorizedAccessException>(
                () => _authService.LoginAsync(loginRequest)
            );
        }

        [Fact]
        public async Task RegisterAsync_UniqueEmail_ReturnsAuthResponse()
        {
            // Arrange
            _mockUserRepo.Setup(r => r.EmailExistsAsync(It.IsAny<string>()))
                .ReturnsAsync(false);
            
            _mockUserRepo.Setup(r => r.CreateAsync(It.IsAny<User>()))
                .ReturnsAsync((User u) => u);

            var registerRequest = new RegisterRequestDto
            {
                Email = "new@test.com",
                Password = "password123",
                FullName = "New User",
                Role = "EndUser"
            };

            // Act
            var result = await _authService.RegisterAsync(registerRequest);

            // Assert
            Assert.NotNull(result);
            Assert.NotNull(result.Token);
            Assert.Equal(registerRequest.Email, result.User.Email);
        }

        [Fact]
        public async Task RegisterAsync_DuplicateEmail_ThrowsInvalidOperationException()
        {
            // Arrange
            _mockUserRepo.Setup(r => r.EmailExistsAsync(It.IsAny<string>()))
                .ReturnsAsync(true);

            var registerRequest = new RegisterRequestDto
            {
                Email = "existing@test.com",
                Password = "password123",
                FullName = "New User",
                Role = "EndUser"
            };

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(
                () => _authService.RegisterAsync(registerRequest)
            );
        }
    }
}
