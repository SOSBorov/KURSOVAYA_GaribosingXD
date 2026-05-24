using WarehouseInventory.Desktop.Models;

namespace WarehouseInventory.Desktop.Services.Api;

public sealed class AuthApiResult
{
    public bool Succeeded { get; init; }

    public string ErrorMessage { get; init; } = string.Empty;

    public UserSessionProfile? Profile { get; init; }

    public static AuthApiResult Success(UserSessionProfile profile) =>
        new()
        {
            Succeeded = true,
            Profile = profile
        };

    public static AuthApiResult Failure(string errorMessage) =>
        new()
        {
            Succeeded = false,
            ErrorMessage = errorMessage
        };
}
