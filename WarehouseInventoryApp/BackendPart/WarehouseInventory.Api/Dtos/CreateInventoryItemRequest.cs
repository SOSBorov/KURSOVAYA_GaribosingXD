using System.ComponentModel.DataAnnotations;

namespace WarehouseInventory.Api.Dtos;

public sealed class CreateInventoryItemRequest
{
    [Required]
    [StringLength(120, MinimumLength = 2)]
    public string Name { get; init; } = string.Empty;

    [Required]
    [StringLength(50, MinimumLength = 3)]
    public string Sku { get; init; } = string.Empty;

    [Required]
    [StringLength(80, MinimumLength = 2)]
    public string Category { get; init; } = string.Empty;

    [Required]
    [StringLength(20, MinimumLength = 1)]
    public string UnitOfMeasure { get; init; } = string.Empty;

    [Range(0, int.MaxValue)]
    public int QuantityInStock { get; init; }

    [Range(typeof(decimal), "0", "999999999")]
    public decimal UnitPrice { get; init; }

    [Required]
    [StringLength(80, MinimumLength = 2)]
    public string WarehouseLocation { get; init; } = string.Empty;
}
