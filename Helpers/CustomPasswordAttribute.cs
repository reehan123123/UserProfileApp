using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace UserProfileApp.Helpers
{
    public class CustomPasswordAttribute : ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            var password = value as string;

            if (string.IsNullOrEmpty(password))
            {
                return new ValidationResult("Password is required.");
            }

            // Check for minimum length
            if (password.Length < 8)
            {
                return new ValidationResult("Password must be at least 8 characters long.");
            }

            // Check for at least one uppercase letter
            if (!Regex.IsMatch(password, "[A-Z]"))
            {
                return new ValidationResult("Password must contain at least one uppercase letter.");
            }

            // Check for at least one lowercase letter
            if (!Regex.IsMatch(password, "[a-z]"))
            {
                return new ValidationResult("Password must contain at least one lowercase letter.");
            }

            // Check for at least one digit
            if (!Regex.IsMatch(password, "[0-9]"))
            {
                return new ValidationResult("Password must contain at least one number.");
            }

            return ValidationResult.Success;
        }
    }
}