using System.ComponentModel.DataAnnotations;
using UserProfileApp.Helpers;
namespace UserProfileApp.Models
{
    public class RegisterViewModel
    {
        [Required]
        [MinLength(3, ErrorMessage = "Username must be at least 3 characters long.")]
        [RegularExpression("^[a-zA-Z0-9]*$", ErrorMessage = "Username can only contain alphanumeric characters.")]
        public string Username { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [CustomPassword(ErrorMessage = "Password must be at least 8 characters long, include at least one uppercase letter, one lowercase letter, and one number.")]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }
    }


}
