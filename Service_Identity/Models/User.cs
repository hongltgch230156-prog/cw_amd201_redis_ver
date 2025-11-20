using System.ComponentModel.DataAnnotations;

namespace Service_Identity.Models
{
    public class User
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string FullName { get; set; } = string.Empty;

        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [DataType(DataType.Date)]
        public DateTime DateOfBirth { get; set; }

        // Nếu dùng Firebase, có thể thêm UID
        public string? FirebaseUid { get; set; }
    }
}