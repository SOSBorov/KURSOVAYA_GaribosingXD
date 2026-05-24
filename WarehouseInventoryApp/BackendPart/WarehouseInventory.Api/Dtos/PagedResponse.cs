namespace WarehouseInventory.Api.Dtos;

public sealed class PagedResponse<T>
{
    public required IReadOnlyCollection<T> Items { get; init; }

    public required int Page { get; init; }

    public required int PageSize { get; init; }

    public required int TotalCount { get; init; }

    public int TotalPages => TotalCount == 0
        ? 0
        : (int)Math.Ceiling(TotalCount / (double)PageSize);
}
