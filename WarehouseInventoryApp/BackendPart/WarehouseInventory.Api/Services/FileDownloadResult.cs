namespace WarehouseInventory.Api.Services;

public sealed class FileDownloadResult
{
    public bool Succeeded { get; init; }

    public int StatusCode { get; init; }

    public string Message { get; init; } = string.Empty;

    public string FilePath { get; init; } = string.Empty;

    public string FileName { get; init; } = string.Empty;

    public string ContentType { get; init; } = string.Empty;

    public static FileDownloadResult Success(string filePath, string fileName, string contentType)
    {
        return new FileDownloadResult
        {
            Succeeded = true,
            StatusCode = StatusCodes.Status200OK,
            FilePath = filePath,
            FileName = fileName,
            ContentType = contentType
        };
    }

    public static FileDownloadResult Failure(int statusCode, string message)
    {
        return new FileDownloadResult
        {
            Succeeded = false,
            StatusCode = statusCode,
            Message = message
        };
    }
}
