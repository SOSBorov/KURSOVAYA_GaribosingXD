namespace WarehouseInventory.Api.Models;

public sealed class InventoryItem
{
    public Guid Id { get; init; }

    public string Name { get; set; } = string.Empty;

    public string Sku { get; set; } = string.Empty;

    public string Category { get; set; } = string.Empty;

    public string UnitOfMeasure { get; set; } = string.Empty;

    public int QuantityInStock { get; set; }

    public decimal UnitPrice { get; set; }

    public string WarehouseLocation { get; set; } = string.Empty;

    public DateTime LastUpdatedUtc { get; set; }
}
