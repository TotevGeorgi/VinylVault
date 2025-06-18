using System.ComponentModel.DataAnnotations;

namespace Common
{
    public class Person
    {
        public Guid UserId { get; set; }

        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Invalid email format.")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Password is required.")]
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\W).{8,}$",
    ErrorMessage = "Password must be at least 8 characters and include upper, lower, and special character.")]
        public string Password { get; set; }


        public string PasswordHash { get; set; }

        [Required(ErrorMessage = "Full name is required.")]
        public string FullName { get; set; }

        public string Address { get; set; } = string.Empty;
        public bool RememberMe { get; set; } = false;
        public string Role { get; set; } = "User";

        public virtual void UpdateProfile(string fullName, string address)
        {
            this.FullName = fullName;
            this.Address = address;
        }
    }
}
