using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WarehouseInventory.Api.Dtos;
using WarehouseInventory.Api.Services;

namespace WarehouseInventory.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/inventory-items")]
public sealed class InventoryItemsController(
    IInventoryItemService inventoryItemService,
    IInventoryItemFileService inventoryItemFileService) : ControllerBase
{
    /// <summary>
    /// Returns inventory items available in the warehouse with pagination and filters.
    /// </summary>
    /// <param name="request">Pagination and filter parameters.</param>
    /// <param name="cancellationToken">Request cancellation token.</param>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResponse<InventoryItemResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResponse<InventoryItemResponse>>> GetAll(
        [FromQuery] InventoryItemsQueryRequest request,
        CancellationToken cancellationToken)
    {
        return Ok(await inventoryItemService.GetAllAsync(request, cancellationToken));
    }

    /// <summary>
    /// Returns one inventory item by its identifier.
    /// </summary>
    /// <param name="id">Inventory item identifier.</param>
    /// <param name="cancellationToken">Request cancellation token.</param>
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

    /// <summary>
    /// Creates a new inventory item.
    /// </summary>
    /// <param name="request">Inventory item data for creation.</param>
    /// <param name="cancellationToken">Request cancellation token.</param>
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

    /// <summary>
    /// Updates an existing inventory item by identifier.
    /// </summary>
    /// <param name="id">Inventory item identifier.</param>
    /// <param name="request">Updated inventory item data.</param>
    /// <param name="cancellationToken">Request cancellation token.</param>
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

    /// <summary>
    /// Deletes an inventory item by identifier.
    /// </summary>
    /// <param name="id">Inventory item identifier.</param>
    /// <param name="cancellationToken">Request cancellation token.</param>
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

    /// <summary>
    /// Uploads a file for the selected inventory item.
    /// </summary>
    /// <param name="id">Inventory item identifier.</param>
    /// <param name="request">Multipart form-data request containing the file.</param>
    /// <param name="cancellationToken">Request cancellation token.</param>
    [HttpPost("{id:guid}/files")]
    [Consumes("multipart/form-data")]
    [ProducesResponseType(typeof(InventoryItemFileResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<InventoryItemFileResponse>> UploadFile(
        Guid id,
        [FromForm] UploadInventoryItemFileRequest request,
        CancellationToken cancellationToken)
    {
        var result = await inventoryItemFileService.UploadAsync(id, request.File, cancellationToken);
        if (!result.Succeeded)
        {
            return StatusCode(
                result.StatusCode,
                CreateProblemDetails(result.StatusCode, "Ошибка загрузки файла", result.Message));
        }

        return CreatedAtRoute(
            "DownloadInventoryItemFile",
            new { id, fileId = result.File!.Id },
            result.File);
    }

    /// <summary>
    /// Returns metadata of files uploaded for the selected inventory item.
    /// </summary>
    /// <param name="id">Inventory item identifier.</param>
    /// <param name="cancellationToken">Request cancellation token.</param>
    [HttpGet("{id:guid}/files")]
    [ProducesResponseType(typeof(IReadOnlyCollection<InventoryItemFileResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyCollection<InventoryItemFileResponse>>> GetFiles(
        Guid id,
        CancellationToken cancellationToken)
    {
        return Ok(await inventoryItemFileService.GetFilesAsync(id, cancellationToken));
    }

    /// <summary>
    /// Downloads a file attached to the selected inventory item.
    /// </summary>
    /// <param name="id">Inventory item identifier.</param>
    /// <param name="fileId">Uploaded file identifier.</param>
    /// <param name="cancellationToken">Request cancellation token.</param>
    [HttpGet("{id:guid}/files/{fileId:guid}", Name = "DownloadInventoryItemFile")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DownloadFile(Guid id, Guid fileId, CancellationToken cancellationToken)
    {
        var result = await inventoryItemFileService.DownloadAsync(id, fileId, cancellationToken);
        if (!result.Succeeded)
        {
            return StatusCode(
                result.StatusCode,
                CreateProblemDetails(result.StatusCode, "Ошибка скачивания файла", result.Message));
        }

        return PhysicalFile(result.FilePath, result.ContentType, result.FileName);
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
