namespace WarehouseInventory.Api.Dtos;

public sealed class AuthResponse
{
    public string Token { get; init; } = string.Empty;

    public DateTime ExpiresAtUtc { get; init; }

    public string UserName { get; init; } = string.Empty;

    public string Email { get; init; } = string.Empty;
}
