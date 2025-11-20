using CrudCoursework.Models;

namespace CrudCoursework.CQRS.Commands
{
    public class CreateUrlCommand
    {
        public string ShortCode { get; set; } = string.Empty;
        public string OriginalUrl { get; set; } = string.Empty;
    }
}
