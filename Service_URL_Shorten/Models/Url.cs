using System.ComponentModel.DataAnnotations;

namespace Service_URL_Shorten.Models
{
    public class Url
    {
        public int Id { get; set; }
        [Required]
        [MaxLength(500)]
        public string OriginalUrl { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        public string ShortCode { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    }
}
