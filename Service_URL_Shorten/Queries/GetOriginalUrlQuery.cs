using MediatR;
using Service_URL_Shorten.Models;
using Service_URL_Shorten.DTOs;
namespace Service_URL_Shorten.Queries

{
    public class GetOriginalUrlQuery : IRequest<UrlResult>
    {
        public string ShortCode { get; set; } = string.Empty;
        public bool IncrementCount { get; set; } = false;
    }
}
