using WarehouseInventory.Api.Dtos;

namespace WarehouseInventory.Api.Services;

public sealed class InventoryItemOperationResult
{
    public bool Succeeded { get; init; }

    public InventoryOperationError Error { get; init; }

    public string Message { get; init; } = string.Empty;

    public InventoryItemResponse? Item { get; init; }

    public static InventoryItemOperationResult Success(InventoryItemResponse item)
    {
        return new InventoryItemOperationResult
        {
            Succeeded = true,
            Item = item
        };
    }

    public static InventoryItemOperationResult Failure(InventoryOperationError error, string message)
    {
        return new InventoryItemOperationResult
        {
            Succeeded = false,
            Error = error,
            Message = message
        };
    }
}
