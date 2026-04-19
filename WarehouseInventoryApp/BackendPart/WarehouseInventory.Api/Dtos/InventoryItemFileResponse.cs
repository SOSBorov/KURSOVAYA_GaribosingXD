namespace WarehouseInventory.Api.Dtos;

public sealed class InventoryItemFileResponse
{
    public Guid Id { get; init; }

    public Guid InventoryItemId { get; init; }

    public string FileName { get; init; } = string.Empty;

    public string ContentType { get; init; } = string.Empty;

    public long SizeInBytes { get; init; }

    public DateTime UploadedAtUtc { get; init; }
}
