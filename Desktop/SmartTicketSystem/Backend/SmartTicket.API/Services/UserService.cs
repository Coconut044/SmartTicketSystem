using Microsoft.EntityFrameworkCore;
using SmartTicket.API.Data;
using SmartTicket.API.DTOs.Request;
using SmartTicket.API.DTOs.Response;
using SmartTicket.API.Models.Entities;

namespace SmartTicket.API.Services
{
    public class UserService : IUserService
    {
        private readonly ApplicationDbContext _context;

        public UserService(ApplicationDbContext context)
        {
            _context = context;
        }

        // ===================== Get all users =====================
        public async Task<List<UserDto>> GetAllUsersAsync()
        {
            return await _context.Users
                .OrderBy(u => u.FullName)
                .Select(u => MapToDto(u))
                .ToListAsync();
        }

        // ===================== Get users by role =====================
        public async Task<List<UserDto>> GetUsersByRoleAsync(string role)
        {
            return await _context.Users
                .Where(u => u.Role == role && u.IsActive)
                .OrderBy(u => u.FullName)
                .Select(u => MapToDto(u))
                .ToListAsync();
        }

        // ===================== Get user by ID =====================
        public async Task<UserDto?> GetUserByIdAsync(int id)
        {
            return await _context.Users
                .Where(u => u.Id == id)
                .Select(u => MapToDto(u))
                .FirstOrDefaultAsync();
        }

        // ===================== Create user =====================
        public async Task<UserDto> CreateUserAsync(CreateUserDto dto)
        {
            var emailExists = await _context.Users
                .AnyAsync(u => u.Email == dto.Email);

            if (emailExists)
                throw new InvalidOperationException("Email already exists");

            var user = new User
            {
                FullName = dto.FullName,
                Email = dto.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
                Role = dto.Role,
                IsActive = dto.IsActive,
                CreatedAt = DateTime.UtcNow
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return MapToDto(user);
        }

        // ===================== Update user =====================
        public async Task<UserDto> UpdateUserAsync(int id, UpdateUserDto dto)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == id);

            if (user == null)
                throw new InvalidOperationException("User not found");

            if (!string.IsNullOrWhiteSpace(dto.FullName))
                user.FullName = dto.FullName;

            if (!string.IsNullOrWhiteSpace(dto.Email))
                user.Email = dto.Email;

            if (!string.IsNullOrWhiteSpace(dto.Role))
                user.Role = dto.Role;

            if (dto.IsActive.HasValue)
                user.IsActive = dto.IsActive.Value;

            if (!string.IsNullOrWhiteSpace(dto.Password))
                user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password);

            await _context.SaveChangesAsync();

            return MapToDto(user);
        }

        // ===================== Delete user =====================
        public async Task<bool> DeleteUserAsync(int id)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == id);

            if (user == null)
                return false;

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();

            return true;
        }

        // ===================== Mapper =====================
        private static UserDto MapToDto(User user)
        {
            return new UserDto
            {
                Id = user.Id,
                FullName = user.FullName,
                Email = user.Email,
                Role = user.Role,
                IsActive = user.IsActive,
                CreatedAt = user.CreatedAt
            };
        }
    }
}
