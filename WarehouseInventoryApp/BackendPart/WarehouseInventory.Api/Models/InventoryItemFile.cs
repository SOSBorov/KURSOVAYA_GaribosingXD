namespace WarehouseInventory.Api.Models;

public sealed class InventoryItemFile
{
    public Guid Id { get; set; }

    public Guid InventoryItemId { get; set; }

    public string OriginalFileName { get; set; } = string.Empty;

    public string StoredFileName { get; set; } = string.Empty;

    public string ContentType { get; set; } = string.Empty;

    public long SizeInBytes { get; set; }

    public DateTime UploadedAtUtc { get; set; }

    public InventoryItem InventoryItem { get; set; } = null!;
}
