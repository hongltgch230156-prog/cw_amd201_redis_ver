using Xunit;
using Microsoft.EntityFrameworkCore;
using Service_URL_Shorten.Commands;
using Service_URL_Shorten.Data;
using System.Threading;
using System.Threading.Tasks;

namespace Test_Shorten_Url.Handlers
{
    public class CreateShortUrlCommandHandlerTests
    {
        private ApplicationDbContext GetDbContext()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: "TestDb_Create")
                .Options;
            return new ApplicationDbContext(options);
        }

        [Fact]
        public async Task Handle_ValidUrl_ShouldCreateShortUrl()
        {
            // Arrange
            var context = GetDbContext();
            var handler = new CreateShortUrlCommandHandler(context);
            var command = new CreateShortUrlCommand { OriginalUrl = "https://example.com" };

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("https://example.com", result.OriginalUrl);
            Assert.False(string.IsNullOrEmpty(result.ShortCode));
        }

        [Fact]
        public async Task Handle_InvalidUrl_ShouldThrowArgumentException()
        {
            // Arrange
            var context = GetDbContext();
            var handler = new CreateShortUrlCommandHandler(context);
            var command = new CreateShortUrlCommand { OriginalUrl = "invalid-url" };

            // Act & Assert
            await Assert.ThrowsAsync<System.ArgumentException>(() => handler.Handle(command, CancellationToken.None));
        }
    }
}