using WarehouseInventory.Api.Dtos;

namespace WarehouseInventory.Api.Services;

public interface IAuthService
{
    Task<(bool Succeeded, string? Error, AuthResponse? Response)> RegisterAsync(
        RegisterRequest request,
        CancellationToken cancellationToken = default);

    Task<(bool Succeeded, string? Error, AuthResponse? Response)> LoginAsync(
        LoginRequest request,
        CancellationToken cancellationToken = default);
}
