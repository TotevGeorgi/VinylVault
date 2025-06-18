using System.ComponentModel.DataAnnotations;

namespace Common.DTOs
{
    public class RegisterDTO
    {
        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Invalid email format.")]
        [RegularExpression(@"^[^@\s]+@[^@\s]+\.[^@\s]+$", ErrorMessage = "Invalid email format.")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Password is required.")]
        [RegularExpression(@"^(?=.*[A-Z])(?=.*\d)(?=.*[\W_]).{8,}$",
            ErrorMessage = "Password must be at least 8 characters long, include an uppercase letter, number, and special character.")]
        public string Password { get; set; }

        [Required(ErrorMessage = "Full name is required.")]
        public string FullName { get; set; }

        public string Address { get; set; } = string.Empty;
    }
}
