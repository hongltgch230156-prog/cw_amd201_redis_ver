using CrudCoursework.CQRS.Commands;
using CrudCoursework.Data;
using CrudCoursework.Models;
using System.Xml.Serialization;

namespace CrudCoursework.CQRS.Handlers
{
    public class CreateUrlHandler
    {
        private readonly UrlDbContext _context;
        public CreateUrlHandler(UrlDbContext context) => _context = context;

        public async Task<UrlMapping> Handle(CreateUrlCommand command)
        {
            var entity = new UrlMapping
            {
                ShortCode = command.ShortCode,
                OriginalUrl = command.OriginalUrl
            };

            _context.Urls.Add(entity);
            await _context.SaveChangesAsync();
            return entity;
        }
    }
}
