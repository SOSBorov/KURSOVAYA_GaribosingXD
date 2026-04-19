using WarehouseInventory.Api.Dtos;

namespace WarehouseInventory.Api.Services;

public sealed class FileUploadResult
{
    public bool Succeeded { get; init; }

    public int StatusCode { get; init; }

    public string Message { get; init; } = string.Empty;

    public InventoryItemFileResponse? File { get; init; }

    public static FileUploadResult Success(InventoryItemFileResponse file)
    {
        return new FileUploadResult
        {
            Succeeded = true,
            StatusCode = StatusCodes.Status201Created,
            File = file
        };
    }

    public static FileUploadResult Failure(int statusCode, string message)
    {
        return new FileUploadResult
        {
            Succeeded = false,
            StatusCode = statusCode,
            Message = message
        };
    }
}
