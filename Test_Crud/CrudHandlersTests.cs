using Microsoft.EntityFrameworkCore;
using Xunit;
using CrudCoursework.Controllers;
using CrudCoursework.CQRS.Handlers;
using CrudCoursework.Data;
using CrudCoursework.Models;
using CrudCoursework.CQRS.Commands;
using CrudCoursework.CQRS.Queries;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using System.Collections.Generic;

public class UrlControllerTests_InMemory
{
    private UrlDbContext GetDbContext()
    {
        var options = new DbContextOptionsBuilder<UrlDbContext>()
            .UseInMemoryDatabase(databaseName: "TestDb_" + System.Guid.NewGuid())
            .Options;
        return new UrlDbContext(options);
    }

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
    public async Task CreateAndGetById_Works()
    {
        var context = GetDbContext();
        var controller = CreateController(context);

        // Create URL
        var createCmd = new CreateUrlCommand { OriginalUrl = "https://example.com", ShortCode = "abc123" };
        var createdResult = await controller.Create(createCmd) as CreatedAtActionResult;
        var createdUrl = createdResult!.Value as UrlMapping;

        // Get URL by Id
        var getResult = await controller.GetById(createdUrl!.Id) as OkObjectResult;
        var url = getResult!.Value as UrlMapping;

        Assert.Equal("https://example.com", url!.OriginalUrl);
        Assert.Equal("abc123", url.ShortCode);
    }

    [Fact]
    public async Task DeleteAll_RemovesUrls()
    {
        var context = GetDbContext();
        var controller = CreateController(context);

        // Add one URL
        await controller.Create(new CreateUrlCommand { OriginalUrl = "https://test.com", ShortCode = "xyz" });

        // Delete all
        var delResult = await controller.DeleteAll();
        Assert.IsType<NoContentResult>(delResult);

        // Check DB empty
        var allUrls = await context.Urls.ToListAsync();
        Assert.Empty(allUrls);
    }
}