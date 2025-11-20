using MediatR;
using Service_URL_Shorten.Models;
namespace Service_URL_Shorten.Queries
{
    public class GetOriginalUrlQuery : IRequest<Url>
    {
        public string ShortCode { get; set; } = string.Empty;
    }
}
