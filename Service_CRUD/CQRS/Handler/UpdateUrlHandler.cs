using CrudCoursework.CQRS.Commands;
using CrudCoursework.Data;
using CrudCoursework.Models;

namespace CrudCoursework.CQRS.Handlers
{
    public class UpdateUrlHandler
    {
        private readonly UrlDbContext _context;
        public UpdateUrlHandler(UrlDbContext context) => _context = context;

        public async Task<UrlMapping?> Handle(UpdateUrlCommand command)
        {
            var url = await _context.Urls.FindAsync(command.Id);
            if (url == null) return null;

            url.OriginalUrl = command.OriginalUrl;
            url.ShortCode = command.ShortCode;
            await _context.SaveChangesAsync();
            return url;
        }
    }
}
