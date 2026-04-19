using System.Text;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Moq;
using WarehouseInventory.Api.Models;
using WarehouseInventory.Api.Options;
using WarehouseInventory.Api.Services;
using WarehouseInventory.Tests.Helpers;

namespace WarehouseInventory.Tests.Services;

public sealed class InventoryItemFileServiceTests : IDisposable
{
    private readonly string tempRootPath = Path.Combine(Path.GetTempPath(), "warehouse-inventory-tests", Guid.NewGuid().ToString());

    [Fact]
    public async Task UploadAsync_ShouldReturnBadRequest_WhenContentTypeIsNotAllowed()
    {
        await using var dbContext = TestDbContextFactory.Create();
        var itemId = Guid.NewGuid();
        dbContext.InventoryItems.Add(new InventoryItem
        {
            Id = itemId,
            Name = "Printer",
            Sku = "PRN-001",
            Category = "Office",
            UnitOfMeasure = "pcs",
            QuantityInStock = 2,
            UnitPrice = 5000,
            WarehouseLocation = "D-01",
            LastUpdatedUtc = DateTime.UtcNow
        });
        await dbContext.SaveChangesAsync();

        var service = CreateService(dbContext);
        var file = CreateFormFile("manual.exe", "application/octet-stream", "binary-content");

        var result = await service.UploadAsync(itemId, file);

        Assert.False(result.Succeeded);
        Assert.Equal(StatusCodes.Status400BadRequest, result.StatusCode);
        Assert.Null(result.File);
    }

    [Fact]
    public async Task DownloadAsync_ShouldReturnNotFound_WhenFileDoesNotExistOnDisk()
    {
        await using var dbContext = TestDbContextFactory.Create();
        var itemId = Guid.NewGuid();
        var fileId = Guid.NewGuid();

        dbContext.InventoryItems.Add(new InventoryItem
        {
            Id = itemId,
            Name = "Scanner",
            Sku = "SCN-001",
            Category = "Office",
            UnitOfMeasure = "pcs",
            QuantityInStock = 1,
            UnitPrice = 8000,
            WarehouseLocation = "D-02",
            LastUpdatedUtc = DateTime.UtcNow
        });
        dbContext.InventoryItemFiles.Add(new InventoryItemFile
        {
            Id = fileId,
            InventoryItemId = itemId,
            OriginalFileName = "passport.pdf",
            StoredFileName = "missing-file.pdf",
            ContentType = "application/pdf",
            SizeInBytes = 1024,
            UploadedAtUtc = DateTime.UtcNow
        });
        await dbContext.SaveChangesAsync();

        var service = CreateService(dbContext);

        var result = await service.DownloadAsync(itemId, fileId);

        Assert.False(result.Succeeded);
        Assert.Equal(StatusCodes.Status404NotFound, result.StatusCode);
    }

    private InventoryItemFileService CreateService(WarehouseInventory.Api.Data.ApplicationDbContext dbContext)
    {
        Directory.CreateDirectory(tempRootPath);

        var environment = new Mock<IWebHostEnvironment>();
        environment.SetupGet(current => current.ContentRootPath).Returns(tempRootPath);

        var options = Options.Create(new FileStorageOptions
        {
            RootPath = "UploadedFiles",
            MaxFileSizeInBytes = 5 * 1024 * 1024,
            AllowedContentTypes =
            [
                "application/pdf",
                "image/jpeg",
                "image/png",
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet"
            ]
        });

        return new InventoryItemFileService(dbContext, environment.Object, options);
    }

    private static IFormFile CreateFormFile(string fileName, string contentType, string content)
    {
        var bytes = Encoding.UTF8.GetBytes(content);
        var stream = new MemoryStream(bytes);
        return new FormFile(stream, 0, bytes.Length, "file", fileName)
        {
            Headers = new HeaderDictionary(),
            ContentType = contentType
        };
    }

    public void Dispose()
    {
        if (Directory.Exists(tempRootPath))
        {
            Directory.Delete(tempRootPath, recursive: true);
        }
    }
}
