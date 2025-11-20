using CrudCoursework.CQRS.Queries;
using CrudCoursework.Data;
using CrudCoursework.Models;
namespace CrudCoursework.CQRS.Handlers
{
    public class GetUrlByIdHandler
    {
        private readonly UrlDbContext _context;
        public GetUrlByIdHandler(UrlDbContext context) => _context = context;

        public async Task<UrlMapping?> Handle(GetUrlByIdQuery query)
        {
            return await _context.Urls.FindAsync(query.Id);
        }
    }
}
