using CrudCoursework.CQRS.Queries;
using CrudCoursework.Data;
using CrudCoursework.Models;
using Microsoft.EntityFrameworkCore;
using System.Xml.Serialization;

namespace CrudCoursework.CQRS.Handlers
{
    public class GetAllUrlsHandler
    {
        private readonly UrlDbContext _context;
        public GetAllUrlsHandler(UrlDbContext context) => _context = context;

        public async Task<List<UrlMapping>> Handle(GetAllUrlsQuery query)
        {
            return await _context.Urls.ToListAsync();
        }
    }
}
