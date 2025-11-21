using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Service_URL_Shorten.Data;
using Service_URL_Shorten.Models;
using System.Text.Json;
using Service_URL_Shorten.DTOs;

namespace Service_URL_Shorten.Queries
{
    public class GetOriginalUrlQueryHandler : IRequestHandler<GetOriginalUrlQuery, UrlResult>
    {
        private readonly ApplicationDbContext _context;
        private readonly IDistributedCache _cache;

        public GetOriginalUrlQueryHandler(ApplicationDbContext context, IDistributedCache cache)
        {
            _context = context;
            _cache = cache;
        }

        public async Task<UrlResult> Handle(GetOriginalUrlQuery request, CancellationToken cancellationToken)
        {
            string cacheKey = $"url_shorten:{request.ShortCode}";

            // Chỉ đọc từ Cache
            string? cachedData = await _cache.GetStringAsync(cacheKey, cancellationToken);
            if (!string.IsNullOrEmpty(cachedData))
            {
                var urlObj = JsonSerializer.Deserialize<Url>(cachedData)!;
                return new UrlResult
                {
                    OriginalUrl = urlObj.OriginalUrl,
                    Source = "Redis",
                    ClickCount = urlObj.ClickCount
                };
            }

            // Cache miss -> lấy từ DB
            var url = await _context.Urls.FirstOrDefaultAsync(x => x.ShortCode == request.ShortCode);

            if (url != null)
            {
                // Lưu cache lại
                await _cache.SetStringAsync(
                    cacheKey,
                    JsonSerializer.Serialize(url),
                    new DistributedCacheEntryOptions
                    {
                        AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(1),
                        SlidingExpiration = TimeSpan.FromMinutes(10)
                    },
                    cancellationToken);

                return new UrlResult
                {
                    OriginalUrl = url.OriginalUrl,
                    Source = "Database",
                    ClickCount = url.ClickCount
                };
            }

            return null;
        }

    }
}