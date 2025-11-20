using Xunit;
using Microsoft.EntityFrameworkCore;
using Service_URL_Shorten.Data;
using Service_URL_Shorten.Models;
using Service_URL_Shorten.Queries;
using System.Threading;
using System.Threading.Tasks;

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

        [Fact]
        public async Task Handle_ExistingShortCode_ShouldReturnUrl()
        {
            // Arrange
            var context = GetDbContext();
            var handler = new GetOriginalUrlQueryHandler(context);
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
            var handler = new GetOriginalUrlQueryHandler(context);
            var query = new GetOriginalUrlQuery { ShortCode = "notfound" };

            // Act
            var result = await handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.Null(result);
        }
    }
}