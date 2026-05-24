namespace WarehouseInventory.Desktop.Models;

public sealed class AuthSessionRecord
{
    public string Token { get; init; } = string.Empty;

    public DateTime ExpiresAtUtc { get; init; }

    public string UserName { get; init; } = string.Empty;

    public string Email { get; init; } = string.Empty;

    public string EmployeeFullName { get; init; } = string.Empty;
}
