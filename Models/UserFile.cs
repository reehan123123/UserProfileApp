using System.ComponentModel.DataAnnotations;

namespace UserProfileApp.Models
{
    public class UserFile
    {
        [Key]
        public string FileName { get; set; }
        public string FilePath { get; set; }
    }
}
