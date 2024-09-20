using System.ComponentModel.DataAnnotations;

namespace UserProfileApp.Models
{
    public class UserProfile
    {
        [Key]
        public string UserId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Address { get; set; }

        [Phone(ErrorMessage = "Please enter a valid phone number.")]
        [RegularExpression(@"^\+?[0-9]{10,15}$", ErrorMessage = "Phone number must be between 10 and 15 digits.")]
        public string PhoneNumber { get; set; }
        public string ProfilePicturePath { get; set; } = string.Empty;
        public List<UserFile> Files { get; set; } = new List<UserFile>();

    }

}
