using MediatR;
using Service_URL_Shorten.Models;
using Service_URL_Shorten.Data;
using Microsoft.EntityFrameworkCore;
namespace Service_URL_Shorten.Commands
{
    public class CreateShortUrlCommandHandler : IRequestHandler<CreateShortUrlCommand, Url>
    {
        private readonly ApplicationDbContext _context;
        public CreateShortUrlCommandHandler(ApplicationDbContext context)
        {
            _context = context;
        }
        public async Task<Url> Handle(CreateShortUrlCommand request, CancellationToken cancellationToken )
        {
            //validate url
            if(!Uri.IsWellFormedUriString(request.OriginalUrl, UriKind.Absolute))
                throw new ArgumentException("Invalid URL format");
            //generate short code (6 random chars) until unique
            string shortCode;
            //int attempt = 0;
            do
            {
                shortCode = Guid.NewGuid().ToString("N").Substring(0, 6);
                //if (attempt == 0)
                //shortCode = "b6dccc"; // test trùng shortcode
                //else
                //shortCode = Guid.NewGuid().ToString("N").Substring(0, 6);

                //attempt++;
            }
            while (await _context.Urls.AnyAsync(u => u.ShortCode == shortCode, cancellationToken));
            //create new Url entity
            var url = new Url
            {
                OriginalUrl = request.OriginalUrl,
                ShortCode = shortCode
            };

            _context.Urls.Add(url);
            await _context.SaveChangesAsync(cancellationToken);

            return url;
        }

    }
}
