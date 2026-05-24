namespace WarehouseInventory.Desktop.Services.Api;

public sealed class InventoryApiResult<T>
{
    public bool Succeeded { get; init; }

    public string ErrorMessage { get; init; } = string.Empty;

    public int? StatusCode { get; init; }

    public T? Value { get; init; }

    public static InventoryApiResult<T> Success(T value) =>
        new()
        {
            Succeeded = true,
            Value = value
        };

    public static InventoryApiResult<T> Failure(string errorMessage, int? statusCode = null) =>
        new()
        {
            Succeeded = false,
            ErrorMessage = errorMessage,
            StatusCode = statusCode
        };
}
