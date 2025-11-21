using Xunit;
using Microsoft.EntityFrameworkCore;
using Service_URL_Shorten.Data;
using Service_URL_Shorten.Models;
using Service_URL_Shorten.Queries;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Distributed;
using Moq;

namespace Test_Shorten_Url.Handlers
{
    public class GetOriginalUrlQueryHandlerTests
    {
        private ApplicationDbContext GetDbContext()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: "TestDb_Get")
                .Options;

            var context = new ApplicationDbContext(options);

            context.Urls.Add(new Url { OriginalUrl = "https://example.com", ShortCode = "abc123" });
            context.SaveChanges();

            return context;
        }

        private IDistributedCache GetFakeCache()
        {
            var cache = new Mock<IDistributedCache>();

            // Khi gọi GetAsync -> trả về null (để buộc handler đọc DB)
            cache.Setup(c => c.GetAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                 .ReturnsAsync((byte[])null);

            return cache.Object;
        }

        [Fact]
        public async Task Handle_ExistingShortCode_ShouldReturnUrl()
        {
            // Arrange
            var context = GetDbContext();
            var cache = GetFakeCache();

            var handler = new GetOriginalUrlQueryHandler(context, cache);

            var query = new GetOriginalUrlQuery { ShortCode = "abc123" };

            // Act
            var result = await handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("https://example.com", result.OriginalUrl);
        }

        [Fact]
        public async Task Handle_NonExistingShortCode_ShouldReturnNull()
        {
            // Arrange
            var context = GetDbContext();
            var cache = GetFakeCache();

            var handler = new GetOriginalUrlQueryHandler(context, cache);
            var query = new GetOriginalUrlQuery { ShortCode = "notfound" };

            // Act
            var result = await handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.Null(result);
        }
    }
}