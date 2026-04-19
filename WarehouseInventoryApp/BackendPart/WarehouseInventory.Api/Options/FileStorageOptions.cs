namespace WarehouseInventory.Api.Options;

public sealed class FileStorageOptions
{
    public const string SectionName = "FileStorage";

    public string RootPath { get; init; } = "UploadedFiles";

    public long MaxFileSizeInBytes { get; init; } = 5 * 1024 * 1024;

    public string[] AllowedContentTypes { get; init; } = [];
}
