using MediatR;
using Service_URL_Shorten.Models;
using Service_URL_Shorten.Data;
using Microsoft.EntityFrameworkCore;
namespace Service_URL_Shorten.Queries
{
    public class GetOriginalUrlQueryHandler : IRequestHandler<GetOriginalUrlQuery, Url>
    {
        private readonly ApplicationDbContext _context;

        public GetOriginalUrlQueryHandler(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Url?> Handle(GetOriginalUrlQuery request, CancellationToken cancellationToken)
        {
            return await _context.Urls
                .FirstOrDefaultAsync(x => x.ShortCode == request.ShortCode, cancellationToken);
        }
    }
}
