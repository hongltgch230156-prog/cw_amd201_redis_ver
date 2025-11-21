using MediatR;
namespace Service_URL_Shorten.Commands
{
    public class IncreaseClickCountCommand : IRequest<bool>
    {
        public string ShortCode { get; set; }
        public bool IncrementCount { get; set; } = false;
    }
}
