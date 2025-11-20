using Xunit;
using Microsoft.EntityFrameworkCore;
using CrudCoursework.Data;
using CrudCoursework.Models;
using CrudCoursework.CQRS.Handlers;
using CrudCoursework.CQRS.Commands;
using CrudCoursework.CQRS.Queries;
using CrudCoursework.Controllers;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace Test_Crud.Controllers
{
    public class UrlControllerTests
    {
        // Tạo DbContext InMemory mới mỗi lần test (tách biệt)
        private UrlDbContext GetInMemoryDbContext()
        {
            var options = new DbContextOptionsBuilder<UrlDbContext>()
                .UseInMemoryDatabase(databaseName: System.Guid.NewGuid().ToString())
                .Options;
            return new UrlDbContext(options);
        }

        // Tạo controller với DI handlers
        private UrlController CreateController(UrlDbContext context)
        {
            return new UrlController(
                new GetAllUrlsHandler(context),
                new GetUrlByIdHandler(context),
                new CreateUrlHandler(context),
                new UpdateUrlHandler(context),
                new DeleteUrlHandler(context),
                new DeleteAllUrlsHandler(context)
            );
        }

        [Fact]
        public async Task Create_Should_Add_New_Url()
        {
            var context = GetInMemoryDbContext();
            var controller = CreateController(context);

            var command = new CreateUrlCommand
            {
                ShortCode = "abc123",
                OriginalUrl = "https://google.com"
            };

            var action = await controller.Create(command);

            var result = action as CreatedAtActionResult;
            Assert.NotNull(result);

            var created = result.Value as UrlMapping;
            Assert.NotNull(created);
            Assert.Equal("abc123", created.ShortCode);

            // Đây là chỗ fix: database luôn mới → chỉ 1 item
            Assert.Single(context.Urls);
        }

        [Fact]
        public async Task GetAll_Should_Return_All_Urls()
        {
            var context = GetInMemoryDbContext();

            context.Urls.Add(new UrlMapping { ShortCode = "a1", OriginalUrl = "url1" });
            context.Urls.Add(new UrlMapping { ShortCode = "b2", OriginalUrl = "url2" });
            await context.SaveChangesAsync();

            var controller = CreateController(context);

            var action = await controller.GetAll();
            var result = action as OkObjectResult;
            Assert.NotNull(result);

            var list = result.Value as List<UrlMapping>;
            Assert.Equal(2, list.Count);
        }

        [Fact]
        public async Task GetById_Should_Return_Specific_Url()
        {
            var context = GetInMemoryDbContext();

            var entity = new UrlMapping { ShortCode = "x1", OriginalUrl = "test" };
            context.Urls.Add(entity);
            await context.SaveChangesAsync();

            var controller = CreateController(context);

            var action = await controller.GetById(entity.Id);
            var result = action as OkObjectResult;
            Assert.NotNull(result);

            var url = result.Value as UrlMapping;
            Assert.Equal("x1", url.ShortCode);
        }

        [Fact]
        public async Task Update_Should_Modify_Existing_Url()
        {
            var context = GetInMemoryDbContext();

            var entity = new UrlMapping { ShortCode = "old", OriginalUrl = "oldurl" };
            context.Urls.Add(entity);
            await context.SaveChangesAsync();

            var controller = CreateController(context);

            var command = new UpdateUrlCommand
            {
                Id = entity.Id,
                ShortCode = "new",
                OriginalUrl = "newurl"
            };

            var action = await controller.Update(entity.Id, command);
            var result = action as OkObjectResult;

            Assert.NotNull(result);

            var updated = result.Value as UrlMapping;
            Assert.Equal("new", updated.ShortCode);
            Assert.Equal("newurl", updated.OriginalUrl);
        }

        [Fact]
        public async Task Delete_Should_Remove_By_ShortCode()
        {
            var context = GetInMemoryDbContext();

            var entity = new UrlMapping { ShortCode = "del", OriginalUrl = "old" };
            context.Urls.Add(entity);
            await context.SaveChangesAsync();

            var controller = CreateController(context);

            var action = await controller.Delete("del");
            Assert.IsType<NoContentResult>(action);

            Assert.Empty(context.Urls);
        }

        [Fact]
        public async Task DeleteAll_Should_Remove_All()
        {
            var context = GetInMemoryDbContext();

            context.Urls.Add(new UrlMapping { ShortCode = "a", OriginalUrl = "1" });
            context.Urls.Add(new UrlMapping { ShortCode = "b", OriginalUrl = "2" });
            await context.SaveChangesAsync();

            var controller = CreateController(context);

            var action = await controller.DeleteAll();
            Assert.IsType<NoContentResult>(action);

            Assert.Empty(context.Urls);
        }
    }
}