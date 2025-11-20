using CrudCoursework.CQRS.Commands;
using CrudCoursework.Data;
using Microsoft.EntityFrameworkCore;

namespace CrudCoursework.CQRS.Handlers
{
    public class DeleteAllUrlsHandler
    {
        private readonly UrlDbContext _context;

        public DeleteAllUrlsHandler(UrlDbContext context)
        {
            _context = context;
        }

        public async Task<bool> Handle(DeleteAllUrlsCommand command)
        {
            var allUrls = await _context.Urls.ToListAsync();

            if (!allUrls.Any())
                return false;

            _context.Urls.RemoveRange(allUrls);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
