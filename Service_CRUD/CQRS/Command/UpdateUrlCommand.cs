using CrudCoursework.Models;

namespace CrudCoursework.CQRS.Commands
{
    public class UpdateUrlCommand
    {
        public int Id { get; set; }
        public string ShortCode { get; set; } = string.Empty;
        public string OriginalUrl { get; set; } = string.Empty;
    }
}

