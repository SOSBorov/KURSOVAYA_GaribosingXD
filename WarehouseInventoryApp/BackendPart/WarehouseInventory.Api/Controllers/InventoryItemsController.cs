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
    public async Task<ActionResult<IReadOnlyCollection<InventoryItemResponse>>> GetAll(CancellationToken cancellationToken)
    {
        return Ok(await inventoryItemService.GetAllAsync(cancellationToken));
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<InventoryItemResponse>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var item = await inventoryItemService.GetByIdAsync(id, cancellationToken);
        return item is null ? NotFound() : Ok(item);
    }

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<InventoryItemResponse>> Create(
        CreateInventoryItemRequest request,
        CancellationToken cancellationToken)
    {
        var createdItem = await inventoryItemService.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = createdItem.Id }, createdItem);
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<InventoryItemResponse>> Update(
        Guid id,
        UpdateInventoryItemRequest request,
        CancellationToken cancellationToken)
    {
        var updatedItem = await inventoryItemService.UpdateAsync(id, request, cancellationToken);
        return updatedItem is null ? NotFound() : Ok(updatedItem);
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        return await inventoryItemService.DeleteAsync(id, cancellationToken) ? NoContent() : NotFound();
    }
}
