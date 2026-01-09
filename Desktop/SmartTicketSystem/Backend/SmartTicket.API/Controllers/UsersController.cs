using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using SmartTicket.API.DTOs.Request;
using SmartTicket.API.DTOs.Response;
using SmartTicket.API.Services;

namespace SmartTicket.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class UsersController : ControllerBase
    {
        private readonly IUserService _userService;

        public UsersController(IUserService userService)
        {
            _userService = userService;
        }

        /// <summary>
        /// Get all users - Admin only
        /// </summary>
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<ApiResponseDto<List<UserDto>>>> GetAllUsers()
        {
            var users = await _userService.GetAllUsersAsync();
            return Ok(new ApiResponseDto<List<UserDto>>
            {
                Success = true,
                Message = "Users retrieved successfully",
                Data = users
            });
        }

        /// <summary>
        /// Get users by role
        /// - Admin: Can get users of any role
        /// - SupportManager: Can get only SupportAgents (for assignment purposes)
        /// </summary>
        [HttpGet("role/{role}")]
        public async Task<ActionResult<ApiResponseDto<List<UserDto>>>> GetUsersByRole(string role)
        {
            var currentUserRole = GetCurrentUserRole();

            // ✅ RBAC: Permission check
            if (currentUserRole == "SupportManager")
            {
                // Support Managers can only get agents for assignment
                if (role != "SupportAgent")
                {
                    return Forbid();
                }
            }
            else if (currentUserRole != "Admin")
            {
                // Only Admin and SupportManager can use this endpoint
                return Forbid();
            }

            var users = await _userService.GetUsersByRoleAsync(role);
            return Ok(new ApiResponseDto<List<UserDto>>
            {
                Success = true,
                Message = $"Users with role '{role}' retrieved successfully",
                Data = users
            });
        }

        /// <summary>
        /// Get user by ID
        /// - Admin: Can view any user
        /// - SupportManager: Can view agents
        /// - Others: Can only view their own profile
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<ApiResponseDto<UserDto>>> GetUserById(int id)
        {
            var currentUserId = GetCurrentUserId();
            var currentUserRole = GetCurrentUserRole();

            // ✅ RBAC: Permission check
            if (currentUserRole == "EndUser" || currentUserRole == "SupportAgent")
            {
                if (currentUserId != id)
                {
                    return Forbid(); // Can only view own profile
                }
            }
            // Admin and SupportManager can view any user

            var user = await _userService.GetUserByIdAsync(id);
            if (user == null)
            {
                return NotFound(new ApiResponseDto<UserDto>
                {
                    Success = false,
                    Message = "User not found"
                });
            }

            return Ok(new ApiResponseDto<UserDto>
            {
                Success = true,
                Message = "User retrieved successfully",
                Data = user
            });
        }

        /// <summary>
        /// Create new user - Admin only
        /// </summary>
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<ApiResponseDto<UserDto>>> CreateUser([FromBody] CreateUserDto dto)
        {
            try
            {
                var user = await _userService.CreateUserAsync(dto);
                
                return CreatedAtAction(nameof(GetUserById), new { id = user.Id }, 
                    new ApiResponseDto<UserDto>
                    {
                        Success = true,
                        Message = "User created successfully",
                        Data = user
                    });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new ApiResponseDto<UserDto>
                {
                    Success = false,
                    Message = ex.Message
                });
            }
        }

        /// <summary>
        /// Update user
        /// - Admin: Can update any user and change roles
        /// - Others: Can only update their own profile (limited fields)
        /// </summary>
        [HttpPut("{id}")]
        public async Task<ActionResult<ApiResponseDto<UserDto>>> UpdateUser(
            int id, 
            [FromBody] UpdateUserDto dto)
        {
            try
            {
                var currentUserId = GetCurrentUserId();
                var currentUserRole = GetCurrentUserRole();

                // ✅ RBAC: Permission check
                if (currentUserRole != "Admin")
                {
                    // Non-admins can only update themselves
                    if (currentUserId != id)
                    {
                        return Forbid();
                    }
                    
                    // Non-admins cannot change their own role or active status
                    if (dto.Role != null || dto.IsActive.HasValue)
                    {
                        return BadRequest(new ApiResponseDto<UserDto>
                        {
                            Success = false,
                            Message = "You cannot change your role or active status"
                        });
                    }
                }

                var user = await _userService.UpdateUserAsync(id, dto);
                
                return Ok(new ApiResponseDto<UserDto>
                {
                    Success = true,
                    Message = "User updated successfully",
                    Data = user
                });
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(new ApiResponseDto<UserDto>
                {
                    Success = false,
                    Message = ex.Message
                });
            }
        }

        /// <summary>
        /// Delete user - Admin only
        /// </summary>
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<ApiResponseDto<bool>>> DeleteUser(int id)
        {
            var currentUserId = GetCurrentUserId();

            // Prevent self-deletion
            if (currentUserId == id)
            {
                return BadRequest(new ApiResponseDto<bool>
                {
                    Success = false,
                    Message = "You cannot delete your own account"
                });
            }

            var result = await _userService.DeleteUserAsync(id);
            if (!result)
            {
                return NotFound(new ApiResponseDto<bool>
                {
                    Success = false,
                    Message = "User not found"
                });
            }

            return Ok(new ApiResponseDto<bool>
            {
                Success = true,
                Message = "User deleted successfully",
                Data = true
            });
        }

        /// <summary>
        /// Get current user profile
        /// </summary>
        [HttpGet("me")]
        public async Task<ActionResult<ApiResponseDto<UserDto>>> GetCurrentUserProfile()
        {
            var userId = GetCurrentUserId();
            var user = await _userService.GetUserByIdAsync(userId);
            
            if (user == null)
            {
                return NotFound(new ApiResponseDto<UserDto>
                {
                    Success = false,
                    Message = "User not found"
                });
            }

            return Ok(new ApiResponseDto<UserDto>
            {
                Success = true,
                Message = "Profile retrieved successfully",
                Data = user
            });
        }

        /// <summary>
        /// Get available support agents - Admin and SupportManager only
        /// Used for ticket assignment
        /// </summary>
        [HttpGet("agents/available")]
        [Authorize(Roles = "Admin,SupportManager")]
        public async Task<ActionResult<ApiResponseDto<List<UserDto>>>> GetAvailableAgents()
        {
            var agents = await _userService.GetUsersByRoleAsync("SupportAgent");
            var activeAgents = agents.Where(a => a.IsActive).ToList();
            
            return Ok(new ApiResponseDto<List<UserDto>>
            {
                Success = true,
                Message = "Available agents retrieved successfully",
                Data = activeAgents
            });
        }

        // Helper methods
        private int GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            return int.Parse(userIdClaim!.Value);
        }

        private string GetCurrentUserRole()
        {
            var roleClaim = User.FindFirst(ClaimTypes.Role);
            return roleClaim?.Value ?? "EndUser";
        }
    }
}