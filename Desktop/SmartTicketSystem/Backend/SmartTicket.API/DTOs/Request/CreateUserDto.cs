using System.ComponentModel.DataAnnotations;

namespace SmartTicket.API.DTOs.Request
{
    /// <summary>
    /// DTO for creating a new user (Admin only)
    /// </summary>
    public class CreateUserDto
    {
        [Required(ErrorMessage = "Full name is required")]
        [StringLength(100, ErrorMessage = "Full name cannot exceed 100 characters")]
        public string FullName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        [StringLength(100, ErrorMessage = "Email cannot exceed 100 characters")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Password is required")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Password must be between 6 and 100 characters")]
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "Role is required")]
        [RegularExpression("^(Admin|SupportManager|SupportAgent|EndUser)$", 
            ErrorMessage = "Role must be Admin, SupportManager, SupportAgent, or EndUser")]
        public string Role { get; set; } = "EndUser";

        public bool IsActive { get; set; } = true;
    }
}

    /// <summary>
    /// DTO for updating user information
    /// </summary>
   
   