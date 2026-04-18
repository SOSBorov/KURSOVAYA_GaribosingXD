using WarehouseInventory.Api.Dtos;
using WarehouseInventory.Api.Models;

namespace WarehouseInventory.Api.Services;

public sealed class InventoryItemService : IInventoryItemService
{
    private readonly List<InventoryItem> _items =
    [
        new()
        {
            Id = Guid.NewGuid(),
            Name = "Лазерный сканер штрихкодов",
            Sku = "SCAN-001",
            Category = "Оборудование",
            UnitOfMeasure = "шт",
            QuantityInStock = 12,
            UnitPrice = 7500m,
            WarehouseLocation = "A-01-03",
            LastUpdatedUtc = DateTime.UtcNow
        },
        new()
        {
            Id = Guid.NewGuid(),
            Name = "Термопринтер этикеток",
            Sku = "PRINT-002",
            Category = "Оборудование",
            UnitOfMeasure = "шт",
            QuantityInStock = 6,
            UnitPrice = 18400m,
            WarehouseLocation = "A-02-01",
            LastUpdatedUtc = DateTime.UtcNow
        }
    ];

    private readonly object _syncRoot = new();

    public IReadOnlyCollection<InventoryItemResponse> GetAll()
    {
        lock (_syncRoot)
        {
            return _items
                .OrderBy(item => item.Name)
                .Select(MapToResponse)
                .ToArray();
        }
    }

    public InventoryItemResponse? GetById(Guid id)
    {
        lock (_syncRoot)
        {
            var item = _items.FirstOrDefault(currentItem => currentItem.Id == id);
            return item is null ? null : MapToResponse(item);
        }
    }

    public InventoryItemResponse Create(CreateInventoryItemRequest request)
    {
        lock (_syncRoot)
        {
            var item = new InventoryItem
            {
                Id = Guid.NewGuid(),
                Name = request.Name.Trim(),
                Sku = request.Sku.Trim().ToUpperInvariant(),
                Category = request.Category.Trim(),
                UnitOfMeasure = request.UnitOfMeasure.Trim(),
                QuantityInStock = request.QuantityInStock,
                UnitPrice = request.UnitPrice,
                WarehouseLocation = request.WarehouseLocation.Trim().ToUpperInvariant(),
                LastUpdatedUtc = DateTime.UtcNow
            };

            _items.Add(item);
            return MapToResponse(item);
        }
    }

    public InventoryItemResponse? Update(Guid id, UpdateInventoryItemRequest request)
    {
        lock (_syncRoot)
        {
            var item = _items.FirstOrDefault(currentItem => currentItem.Id == id);
            if (item is null)
            {
                return null;
            }

            item.Name = request.Name.Trim();
            item.Sku = request.Sku.Trim().ToUpperInvariant();
            item.Category = request.Category.Trim();
            item.UnitOfMeasure = request.UnitOfMeasure.Trim();
            item.QuantityInStock = request.QuantityInStock;
            item.UnitPrice = request.UnitPrice;
            item.WarehouseLocation = request.WarehouseLocation.Trim().ToUpperInvariant();
            item.LastUpdatedUtc = DateTime.UtcNow;

            return MapToResponse(item);
        }
    }

    public bool Delete(Guid id)
    {
        lock (_syncRoot)
        {
            var item = _items.FirstOrDefault(currentItem => currentItem.Id == id);
            if (item is null)
            {
                return false;
            }

            _items.Remove(item);
            return true;
        }
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
