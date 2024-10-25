using System.ComponentModel.DataAnnotations;

namespace BadmintonBooking.Models
{
    public class User
    {
        [Key]
        public int UserId { get; set; }

        [Required(ErrorMessage = "Username is required.")]
        public string Username { get; set; }

        [Required(ErrorMessage = "Password is required.")]
        public string Password { get; set; }

        public string Role { get; set; } // Roles: Player, Receptionist, Admin
    }

    public class Player : User
    {
        [Required(ErrorMessage = "Full name is required.")]
        public string FullName { get; set; }

        [Required(ErrorMessage = "Address is required.")]
        public string Address { get; set; }

        [RegularExpression(@"^[a-zA-Z0-9._%+-]+@gmail\.com$", ErrorMessage = "Invalid email format.")]
        public string Email { get; set; }

        [RegularExpression("^[0-9]+$", ErrorMessage = "Phone number must contain digits only.")]
        public string Phone { get; set; }

        [RegularExpression("^[0-9]{13,16}$", ErrorMessage = "Credit card number must be between 13 and 16 digits.")]
        public string CreditCard { get; set; }
    }

    public class GmailEmailAttribute : ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            var email = value as string;
            if (string.IsNullOrEmpty(email))
            {
                return new ValidationResult("Email is required.");
            }
            if (email.EndsWith("@gmail.com"))
            {
                return ValidationResult.Success;
            }
            return new ValidationResult(ErrorMessage ?? "Email must be a Gmail address.");
        }
    }
}
