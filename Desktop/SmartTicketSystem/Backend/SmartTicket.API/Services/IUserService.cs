using SmartTicket.API.DTOs.Request;
using SmartTicket.API.DTOs.Response;

namespace SmartTicket.API.Services
{
    public interface IUserService
    {
        Task<List<UserDto>> GetAllUsersAsync();

        Task<List<UserDto>> GetUsersByRoleAsync(string role);

        Task<UserDto?> GetUserByIdAsync(int id);

        Task<UserDto> CreateUserAsync(CreateUserDto dto);

        Task<UserDto> UpdateUserAsync(int id, UpdateUserDto dto);

        Task<bool> DeleteUserAsync(int id);
    }
}
