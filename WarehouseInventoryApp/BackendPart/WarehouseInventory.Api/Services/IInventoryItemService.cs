using WarehouseInventory.Api.Dtos;

namespace WarehouseInventory.Api.Services;

public interface IInventoryItemService
{
    Task<PagedResponse<InventoryItemResponse>> GetAllAsync(
        InventoryItemsQueryRequest request,
        CancellationToken cancellationToken = default);

    Task<InventoryItemResponse?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<InventoryItemOperationResult> CreateAsync(
        CreateInventoryItemRequest request,
        CancellationToken cancellationToken = default);

    Task<InventoryItemOperationResult> UpdateAsync(
        Guid id,
        UpdateInventoryItemRequest request,
        CancellationToken cancellationToken = default);

    Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
