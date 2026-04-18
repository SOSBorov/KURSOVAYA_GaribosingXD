using Microsoft.AspNetCore.Mvc;
using WarehouseInventory.Api.Dtos;
using WarehouseInventory.Api.Services;

namespace WarehouseInventory.Api.Controllers;

[ApiController]
[Route("api/inventory-items")]
public sealed class InventoryItemsController(IInventoryItemService inventoryItemService) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public ActionResult<IReadOnlyCollection<InventoryItemResponse>> GetAll()
    {
        return Ok(inventoryItemService.GetAll());
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public ActionResult<InventoryItemResponse> GetById(Guid id)
    {
        var item = inventoryItemService.GetById(id);
        return item is null ? NotFound() : Ok(item);
    }

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public ActionResult<InventoryItemResponse> Create(CreateInventoryItemRequest request)
    {
        var createdItem = inventoryItemService.Create(request);
        return CreatedAtAction(nameof(GetById), new { id = createdItem.Id }, createdItem);
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public ActionResult<InventoryItemResponse> Update(Guid id, UpdateInventoryItemRequest request)
    {
        var updatedItem = inventoryItemService.Update(id, request);
        return updatedItem is null ? NotFound() : Ok(updatedItem);
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult Delete(Guid id)
    {
        return inventoryItemService.Delete(id) ? NoContent() : NotFound();
    }
}
