using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WarehouseInventory.Api.Dtos;
using WarehouseInventory.Api.Services;

namespace WarehouseInventory.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/inventory-items")]
public sealed class InventoryItemsController(IInventoryItemService inventoryItemService) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyCollection<InventoryItemResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyCollection<InventoryItemResponse>>> GetAll(CancellationToken cancellationToken)
    {
        return Ok(await inventoryItemService.GetAllAsync(cancellationToken));
    }

    [HttpGet("{id:guid}", Name = "GetInventoryItemById")]
    [ProducesResponseType(typeof(InventoryItemResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<InventoryItemResponse>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var item = await inventoryItemService.GetByIdAsync(id, cancellationToken);
        return item is null
            ? NotFound(CreateProblemDetails(
                StatusCodes.Status404NotFound,
                "Товар не найден",
                $"Позиция с идентификатором '{id}' отсутствует."))
            : Ok(item);
    }

    [HttpPost]
    [ProducesResponseType(typeof(InventoryItemResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<InventoryItemResponse>> Create(
        CreateInventoryItemRequest request,
        CancellationToken cancellationToken)
    {
        var result = await inventoryItemService.CreateAsync(request, cancellationToken);
        if (!result.Succeeded)
        {
            return Conflict(CreateProblemDetails(
                StatusCodes.Status409Conflict,
                "Конфликт данных",
                result.Message));
        }

        return CreatedAtRoute("GetInventoryItemById", new { id = result.Item!.Id }, result.Item);
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(InventoryItemResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<InventoryItemResponse>> Update(
        Guid id,
        UpdateInventoryItemRequest request,
        CancellationToken cancellationToken)
    {
        var result = await inventoryItemService.UpdateAsync(id, request, cancellationToken);
        if (result.Succeeded)
        {
            return Ok(result.Item);
        }

        return result.Error switch
        {
            InventoryOperationError.NotFound => NotFound(CreateProblemDetails(
                StatusCodes.Status404NotFound,
                "Товар не найден",
                result.Message)),
            InventoryOperationError.DuplicateSku => Conflict(CreateProblemDetails(
                StatusCodes.Status409Conflict,
                "Конфликт данных",
                result.Message)),
            _ => BadRequest(CreateProblemDetails(
                StatusCodes.Status400BadRequest,
                "Некорректный запрос",
                "Не удалось обновить товар."))
        };
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        return await inventoryItemService.DeleteAsync(id, cancellationToken)
            ? NoContent()
            : NotFound(CreateProblemDetails(
                StatusCodes.Status404NotFound,
                "Товар не найден",
                $"Позиция с идентификатором '{id}' отсутствует."));
    }

    private ProblemDetails CreateProblemDetails(int statusCode, string title, string detail)
    {
        return new ProblemDetails
        {
            Status = statusCode,
            Title = title,
            Detail = detail,
            Instance = HttpContext.Request.Path
        };
    }
}
