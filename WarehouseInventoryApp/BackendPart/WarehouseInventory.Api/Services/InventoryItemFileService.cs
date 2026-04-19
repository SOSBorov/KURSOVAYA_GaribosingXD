using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using WarehouseInventory.Api.Data;
using WarehouseInventory.Api.Dtos;
using WarehouseInventory.Api.Models;
using WarehouseInventory.Api.Options;

namespace WarehouseInventory.Api.Services;

public sealed class InventoryItemFileService(
    ApplicationDbContext dbContext,
    IWebHostEnvironment environment,
    IOptions<FileStorageOptions> fileStorageOptions) : IInventoryItemFileService
{
    private readonly FileStorageOptions options = fileStorageOptions.Value;

    public async Task<FileUploadResult> UploadAsync(Guid inventoryItemId, IFormFile file, CancellationToken cancellationToken = default)
    {
        var itemExists = await dbContext.InventoryItems.AnyAsync(item => item.Id == inventoryItemId, cancellationToken);
        if (!itemExists)
        {
            return FileUploadResult.Failure(StatusCodes.Status404NotFound, "Товар не найден.");
        }

        if (file.Length == 0)
        {
            return FileUploadResult.Failure(StatusCodes.Status400BadRequest, "Файл пустой.");
        }

        if (file.Length > options.MaxFileSizeInBytes)
        {
            return FileUploadResult.Failure(StatusCodes.Status400BadRequest, "Размер файла превышает допустимый лимит.");
        }

        if (!options.AllowedContentTypes.Contains(file.ContentType, StringComparer.OrdinalIgnoreCase))
        {
            return FileUploadResult.Failure(StatusCodes.Status400BadRequest, "Тип файла не разрешен.");
        }

        var uploadsRoot = Path.Combine(environment.ContentRootPath, options.RootPath);
        Directory.CreateDirectory(uploadsRoot);

        var storedFileName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
        var filePath = Path.Combine(uploadsRoot, storedFileName);

        await using (var stream = File.Create(filePath))
        {
            await file.CopyToAsync(stream, cancellationToken);
        }

        var fileEntity = new InventoryItemFile
        {
            Id = Guid.NewGuid(),
            InventoryItemId = inventoryItemId,
            OriginalFileName = Path.GetFileName(file.FileName),
            StoredFileName = storedFileName,
            ContentType = file.ContentType,
            SizeInBytes = file.Length,
            UploadedAtUtc = DateTime.UtcNow
        };

        dbContext.InventoryItemFiles.Add(fileEntity);
        await dbContext.SaveChangesAsync(cancellationToken);

        return FileUploadResult.Success(MapToResponse(fileEntity));
    }

    public async Task<IReadOnlyCollection<InventoryItemFileResponse>> GetFilesAsync(Guid inventoryItemId, CancellationToken cancellationToken = default)
    {
        return await dbContext.InventoryItemFiles
            .AsNoTracking()
            .Where(file => file.InventoryItemId == inventoryItemId)
            .OrderByDescending(file => file.UploadedAtUtc)
            .Select(file => new InventoryItemFileResponse
            {
                Id = file.Id,
                InventoryItemId = file.InventoryItemId,
                FileName = file.OriginalFileName,
                ContentType = file.ContentType,
                SizeInBytes = file.SizeInBytes,
                UploadedAtUtc = file.UploadedAtUtc
            })
            .ToArrayAsync(cancellationToken);
    }

    public async Task<FileDownloadResult> DownloadAsync(Guid inventoryItemId, Guid fileId, CancellationToken cancellationToken = default)
    {
        var file = await dbContext.InventoryItemFiles
            .AsNoTracking()
            .FirstOrDefaultAsync(
                currentFile => currentFile.InventoryItemId == inventoryItemId && currentFile.Id == fileId,
                cancellationToken);

        if (file is null)
        {
            return FileDownloadResult.Failure(StatusCodes.Status404NotFound, "Файл не найден.");
        }

        var uploadsRoot = Path.Combine(environment.ContentRootPath, options.RootPath);
        var filePath = Path.Combine(uploadsRoot, file.StoredFileName);

        if (!File.Exists(filePath))
        {
            return FileDownloadResult.Failure(StatusCodes.Status404NotFound, "Файл отсутствует в хранилище.");
        }

        return FileDownloadResult.Success(filePath, file.OriginalFileName, file.ContentType);
    }

    private static InventoryItemFileResponse MapToResponse(InventoryItemFile file)
    {
        return new InventoryItemFileResponse
        {
            Id = file.Id,
            InventoryItemId = file.InventoryItemId,
            FileName = file.OriginalFileName,
            ContentType = file.ContentType,
            SizeInBytes = file.SizeInBytes,
            UploadedAtUtc = file.UploadedAtUtc
        };
    }
}
