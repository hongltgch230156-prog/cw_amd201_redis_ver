using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using QRCoder;
using Service_URL_Shorten.Data;

namespace Service_URL_Shorten.Queries
{
    public class GetQrCodeQueryHandler : IRequestHandler<GetQrCodeQuery, byte[]>
    {
        private readonly ApplicationDbContext _context;

        public GetQrCodeQueryHandler(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<byte[]> Handle(GetQrCodeQuery request, CancellationToken cancellationToken)
        {
            // 1. Tìm URL theo shortCode
            var urlEntry = await _context.Urls
                .FirstOrDefaultAsync(u => u.ShortCode == request.ShortCode, cancellationToken);

            if (urlEntry == null)
                throw new Exception("Short code not found.");

            // 2. Không kiểm tra hạn, không phân biệt user
            string fullUrl = urlEntry.OriginalUrl;

            // 3. Generate QR code
            using var qrGenerator = new QRCodeGenerator();
            using var qrData = qrGenerator.CreateQrCode(fullUrl, QRCodeGenerator.ECCLevel.Q);
            var pngByteQrCode = new PngByteQRCode(qrData);

            return pngByteQrCode.GetGraphic(5);
        }
    }

}
