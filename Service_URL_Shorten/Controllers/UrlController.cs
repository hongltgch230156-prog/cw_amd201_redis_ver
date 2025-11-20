using Microsoft.AspNetCore.Mvc;
using MediatR;
using QRCoder;
using Service_URL_Shorten.Queries;
using Service_URL_Shorten.Commands;
namespace Service_URL_Shorten.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UrlController : ControllerBase
    {
        private readonly IMediator _mediator;
        public UrlController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost("shorten")]
        public async Task<IActionResult> ShortenUrl([FromBody] CreateShortUrlCommand command)
        {
            var result = await _mediator.Send(command);
            return Ok(new
            {
                Url = $"{Request.Scheme}://{Request.Host}/api/url/{result.ShortCode}",
                originalUrl = result.OriginalUrl
            });
        }

        [HttpGet("{code}")]
        public async Task<IActionResult> RedirectToOriginal(string code)
        {
            var query = new GetOriginalUrlQuery { ShortCode = code };
            var result = await _mediator.Send(query);
            if (result == null) return NotFound("Short URL not found");
            //return Ok(new { result.OriginalUrl });
            return Redirect(result.OriginalUrl);


        }

        [HttpGet("qr/{shortCode}")]
        public async Task<IActionResult> GetQrCode([FromRoute] string shortCode)
        {
            // 1. Lấy URL gốc
            var originalUrlQuery = new GetOriginalUrlQuery { ShortCode = shortCode };
            var urlResult = await _mediator.Send(originalUrlQuery);

            if (urlResult == null || string.IsNullOrEmpty(urlResult.OriginalUrl))
                return NotFound("Short URL not found or Original URL is empty.");

            // 2. Tạo Query để tạo QR code (không cần UserId, không BaseUrl)
            var qrCodeQuery = new GetQrCodeQuery
            {
                ShortCode = shortCode
            };

            // 3. Lấy byte từ handler
            byte[] qrCodeBytes = await _mediator.Send(qrCodeQuery);

            // 4. Không để client cache
            Response.Headers["Cache-Control"] = "no-store";

            // 5. Trả QR code
            return File(qrCodeBytes, "image/png");
        }


    }
}
