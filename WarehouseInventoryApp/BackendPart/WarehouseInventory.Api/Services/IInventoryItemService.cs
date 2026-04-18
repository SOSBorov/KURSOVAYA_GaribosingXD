using WarehouseInventory.Api.Dtos;

namespace WarehouseInventory.Api.Services;

public interface IInventoryItemService
{
    IReadOnlyCollection<InventoryItemResponse> GetAll();

    InventoryItemResponse? GetById(Guid id);

    InventoryItemResponse Create(CreateInventoryItemRequest request);

    InventoryItemResponse? Update(Guid id, UpdateInventoryItemRequest request);

    bool Delete(Guid id);
}
