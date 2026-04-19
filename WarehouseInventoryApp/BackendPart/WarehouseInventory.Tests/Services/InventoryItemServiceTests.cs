using WarehouseInventory.Api.Dtos;
using WarehouseInventory.Api.Models;
using WarehouseInventory.Api.Services;
using WarehouseInventory.Tests.Helpers;

namespace WarehouseInventory.Tests.Services;

public sealed class InventoryItemServiceTests
{
    [Fact]
    public async Task CreateAsync_ShouldReturnDuplicateSku_WhenSkuAlreadyExists()
    {
        await using var dbContext = TestDbContextFactory.Create();
        dbContext.InventoryItems.Add(new InventoryItem
        {
            Id = Guid.NewGuid(),
            Name = "Existing item",
            Sku = "SKU-001",
            Category = "Electronics",
            UnitOfMeasure = "pcs",
            QuantityInStock = 4,
            UnitPrice = 1500,
            WarehouseLocation = "A-01",
            LastUpdatedUtc = DateTime.UtcNow
        });
        await dbContext.SaveChangesAsync();

        var service = new InventoryItemService(dbContext);
        var request = new CreateInventoryItemRequest
        {
            Name = "New item",
            Sku = "sku-001",
            Category = "Electronics",
            UnitOfMeasure = "pcs",
            QuantityInStock = 1,
            UnitPrice = 1000,
            WarehouseLocation = "B-02"
        };

        var result = await service.CreateAsync(request);

        Assert.False(result.Succeeded);
        Assert.Equal(InventoryOperationError.DuplicateSku, result.Error);
        Assert.Null(result.Item);
    }

    [Fact]
    public async Task UpdateAsync_ShouldReturnNotFound_WhenItemDoesNotExist()
    {
        await using var dbContext = TestDbContextFactory.Create();
        var service = new InventoryItemService(dbContext);

        var request = new UpdateInventoryItemRequest
        {
            Name = "Updated item",
            Sku = "SKU-123",
            Category = "Electronics",
            UnitOfMeasure = "pcs",
            QuantityInStock = 5,
            UnitPrice = 2500,
            WarehouseLocation = "C-03"
        };

        var result = await service.UpdateAsync(Guid.NewGuid(), request);

        Assert.False(result.Succeeded);
        Assert.Equal(InventoryOperationError.NotFound, result.Error);
        Assert.Null(result.Item);
    }
}
