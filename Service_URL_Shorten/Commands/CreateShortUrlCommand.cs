using MediatR;
using Service_URL_Shorten.Models;

namespace Service_URL_Shorten.Commands
{
    public class CreateShortUrlCommand : IRequest<Url>
    {
        public string OriginalUrl { get; set; } = string.Empty;
    }
}
