using Microsoft.EntityFrameworkCore;
using WarehouseInventory.Api.Data;
using WarehouseInventory.Api.Dtos;
using WarehouseInventory.Api.Models;

namespace WarehouseInventory.Api.Services;

public sealed class InventoryItemService(ApplicationDbContext dbContext) : IInventoryItemService
{
    public async Task<PagedResponse<InventoryItemResponse>> GetAllAsync(
        InventoryItemsQueryRequest request,
        CancellationToken cancellationToken = default)
    {
        var query = dbContext.InventoryItems
            .AsNoTracking()
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var normalizedSearch = request.Search.Trim();
            var searchPattern = $"%{normalizedSearch}%";

            query = query.Where(item =>
                EF.Functions.Like(item.Name, searchPattern) ||
                EF.Functions.Like(item.Sku, searchPattern) ||
                EF.Functions.Like(item.Category, searchPattern) ||
                EF.Functions.Like(item.WarehouseLocation, searchPattern));
        }

        if (!string.IsNullOrWhiteSpace(request.Category))
        {
            var normalizedCategory = request.Category.Trim();
            query = query.Where(item => item.Category == normalizedCategory);
        }

        if (!string.IsNullOrWhiteSpace(request.WarehouseLocation))
        {
            var normalizedLocation = request.WarehouseLocation.Trim().ToUpperInvariant();
            query = query.Where(item => item.WarehouseLocation == normalizedLocation);
        }

        var page = request.NormalizedPage;
        var pageSize = request.NormalizedPageSize;
        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderBy(item => item.Name)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(item => new InventoryItemResponse
            {
                Id = item.Id,
                Name = item.Name,
                Sku = item.Sku,
                Category = item.Category,
                UnitOfMeasure = item.UnitOfMeasure,
                QuantityInStock = item.QuantityInStock,
                UnitPrice = item.UnitPrice,
                WarehouseLocation = item.WarehouseLocation,
                LastUpdatedUtc = item.LastUpdatedUtc
            })
            .ToArrayAsync(cancellationToken);

        return new PagedResponse<InventoryItemResponse>
        {
            Items = items,
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount
        };
    }

    public async Task<InventoryItemResponse?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await dbContext.InventoryItems
            .AsNoTracking()
            .Where(item => item.Id == id)
            .Select(item => new InventoryItemResponse
            {
                Id = item.Id,
                Name = item.Name,
                Sku = item.Sku,
                Category = item.Category,
                UnitOfMeasure = item.UnitOfMeasure,
                QuantityInStock = item.QuantityInStock,
                UnitPrice = item.UnitPrice,
                WarehouseLocation = item.WarehouseLocation,
                LastUpdatedUtc = item.LastUpdatedUtc
            })
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<InventoryItemOperationResult> CreateAsync(
        CreateInventoryItemRequest request,
        CancellationToken cancellationToken = default)
    {
        var normalizedSku = request.Sku.Trim().ToUpperInvariant();
        var duplicateSkuExists = await dbContext.InventoryItems
            .AnyAsync(item => item.Sku == normalizedSku, cancellationToken);

        if (duplicateSkuExists)
        {
            return InventoryItemOperationResult.Failure(
                InventoryOperationError.DuplicateSku,
                "Товар с таким SKU уже существует.");
        }

        var item = new InventoryItem
        {
            Id = Guid.NewGuid(),
            Name = request.Name.Trim(),
            Sku = normalizedSku,
            Category = request.Category.Trim(),
            UnitOfMeasure = request.UnitOfMeasure.Trim(),
            QuantityInStock = request.QuantityInStock,
            UnitPrice = request.UnitPrice,
            WarehouseLocation = request.WarehouseLocation.Trim().ToUpperInvariant(),
            LastUpdatedUtc = DateTime.UtcNow
        };

        dbContext.InventoryItems.Add(item);
        await dbContext.SaveChangesAsync(cancellationToken);

        return InventoryItemOperationResult.Success(MapToResponse(item));
    }

    public async Task<InventoryItemOperationResult> UpdateAsync(
        Guid id,
        UpdateInventoryItemRequest request,
        CancellationToken cancellationToken = default)
    {
        var item = await dbContext.InventoryItems.FirstOrDefaultAsync(
            currentItem => currentItem.Id == id,
            cancellationToken);

        if (item is null)
        {
            return InventoryItemOperationResult.Failure(
                InventoryOperationError.NotFound,
                "Товар не найден.");
        }

        var normalizedSku = request.Sku.Trim().ToUpperInvariant();
        var duplicateSkuExists = await dbContext.InventoryItems
            .AnyAsync(currentItem => currentItem.Id != id && currentItem.Sku == normalizedSku, cancellationToken);

        if (duplicateSkuExists)
        {
            return InventoryItemOperationResult.Failure(
                InventoryOperationError.DuplicateSku,
                "Товар с таким SKU уже существует.");
        }

        item.Name = request.Name.Trim();
        item.Sku = normalizedSku;
        item.Category = request.Category.Trim();
        item.UnitOfMeasure = request.UnitOfMeasure.Trim();
        item.QuantityInStock = request.QuantityInStock;
        item.UnitPrice = request.UnitPrice;
        item.WarehouseLocation = request.WarehouseLocation.Trim().ToUpperInvariant();
        item.LastUpdatedUtc = DateTime.UtcNow;

        await dbContext.SaveChangesAsync(cancellationToken);

        return InventoryItemOperationResult.Success(MapToResponse(item));
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var item = await dbContext.InventoryItems.FirstOrDefaultAsync(
            currentItem => currentItem.Id == id,
            cancellationToken);

        if (item is null)
        {
            return false;
        }

        dbContext.InventoryItems.Remove(item);
        await dbContext.SaveChangesAsync(cancellationToken);

        return true;
    }

    private static InventoryItemResponse MapToResponse(InventoryItem item)
    {
        return new InventoryItemResponse
        {
            Id = item.Id,
            Name = item.Name,
            Sku = item.Sku,
            Category = item.Category,
            UnitOfMeasure = item.UnitOfMeasure,
            QuantityInStock = item.QuantityInStock,
            UnitPrice = item.UnitPrice,
            WarehouseLocation = item.WarehouseLocation,
            LastUpdatedUtc = item.LastUpdatedUtc
        };
    }
}
