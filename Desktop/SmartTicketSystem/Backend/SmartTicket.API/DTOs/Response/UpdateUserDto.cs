using System.ComponentModel.DataAnnotations;

public class UpdateUserDto
    {
        [StringLength(100, ErrorMessage = "Full name cannot exceed 100 characters")]
        public string? FullName { get; set; }

        [EmailAddress(ErrorMessage = "Invalid email format")]
        [StringLength(100, ErrorMessage = "Email cannot exceed 100 characters")]
        public string? Email { get; set; }

        [RegularExpression("^(Admin|SupportManager|SupportAgent|EndUser)$", 
            ErrorMessage = "Role must be Admin, SupportManager, SupportAgent, or EndUser")]
        public string? Role { get; set; }

        public bool? IsActive { get; set; }

        [StringLength(100, MinimumLength = 6, ErrorMessage = "Password must be between 6 and 100 characters")]
        public string? Password { get; set; } // Optional: for password updates
    }
