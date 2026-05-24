namespace WarehouseInventory.Api.Dtos;

public sealed class InventoryItemsQueryRequest
{
    private const int DefaultPage = 1;
    private const int DefaultPageSize = 10;
    private const int MaxPageSize = 100;

    public int Page { get; init; } = DefaultPage;

    public int PageSize { get; init; } = DefaultPageSize;

    public string? Search { get; init; }

    public string? Category { get; init; }

    public string? WarehouseLocation { get; init; }

    public int NormalizedPage => Page > 0 ? Page : DefaultPage;

    public int NormalizedPageSize => PageSize switch
    {
        <= 0 => DefaultPageSize,
        > MaxPageSize => MaxPageSize,
        _ => PageSize
    };
}
