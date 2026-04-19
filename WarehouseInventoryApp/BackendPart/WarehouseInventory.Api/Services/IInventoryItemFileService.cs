using Microsoft.AspNetCore.Http;
using WarehouseInventory.Api.Dtos;

namespace WarehouseInventory.Api.Services;

public interface IInventoryItemFileService
{
    Task<FileUploadResult> UploadAsync(Guid inventoryItemId, IFormFile file, CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<InventoryItemFileResponse>> GetFilesAsync(Guid inventoryItemId, CancellationToken cancellationToken = default);

    Task<FileDownloadResult> DownloadAsync(Guid inventoryItemId, Guid fileId, CancellationToken cancellationToken = default);
}
