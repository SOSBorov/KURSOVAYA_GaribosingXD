namespace WarehouseInventory.Api.Dtos;

public sealed class InventoryItemResponse
{
    public Guid Id { get; init; }

    public string Name { get; init; } = string.Empty;

    public string Sku { get; init; } = string.Empty;

    public string Category { get; init; } = string.Empty;

    public string UnitOfMeasure { get; init; } = string.Empty;

    public int QuantityInStock { get; init; }

    public decimal UnitPrice { get; init; }

    public string WarehouseLocation { get; init; } = string.Empty;

    public DateTime LastUpdatedUtc { get; init; }
}
