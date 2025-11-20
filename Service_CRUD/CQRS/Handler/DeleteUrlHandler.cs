using Microsoft.EntityFrameworkCore; // cần import để dùng FirstOrDefaultAsync
using CrudCoursework.CQRS.Commands;
using CrudCoursework.Data;

namespace CrudCoursework.CQRS.Handlers
{
    public class DeleteUrlHandler
    {
        private readonly UrlDbContext _context;
        public DeleteUrlHandler(UrlDbContext context) => _context = context;

        public async Task<bool> Handle(DeleteUrlCommand command)
        {
            var url = await _context.Urls
                .FirstOrDefaultAsync(x => x.ShortCode == command.ShortCode);

            if (url == null) return false;

            _context.Urls.Remove(url);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
