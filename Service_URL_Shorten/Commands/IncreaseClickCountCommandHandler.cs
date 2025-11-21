using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Service_URL_Shorten.Commands;
using Service_URL_Shorten.Data;
using System.Text.Json;

namespace Service_URL_Shorten.Commands
{
    public class IncreaseClickCountCommandHandler : IRequestHandler<IncreaseClickCountCommand, bool>
    {
        private readonly ApplicationDbContext _context;
        private readonly IDistributedCache _cache;

        public IncreaseClickCountCommandHandler(ApplicationDbContext context, IDistributedCache cache)
        {
            _context = context;
            _cache = cache;
        }

        public async Task<bool> Handle(IncreaseClickCountCommand request, CancellationToken cancellationToken)
        {
            // 1. Tìm bản ghi
            var url = await _context.Urls
                .FirstOrDefaultAsync(x => x.ShortCode == request.ShortCode, cancellationToken);

            if (url == null) return false;

            // 2. Tăng ClickCount
            url.ClickCount++;

            // 3. Lưu DB
            await _context.SaveChangesAsync(cancellationToken);

            // 4. Cập nhật lại Redis (QUAN TRỌNG!!!)
            string cacheKey = $"url_shorten:{request.ShortCode}";

            var cacheOptions = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(1),
                SlidingExpiration = TimeSpan.FromMinutes(10)
            };

            await _cache.SetStringAsync(
                cacheKey,
                JsonSerializer.Serialize(url),
                cacheOptions,
                cancellationToken
            );

            return true;
        }
    }
}
