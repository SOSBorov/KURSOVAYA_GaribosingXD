using System.ComponentModel.DataAnnotations;

namespace WarehouseInventory.Api.Dtos;

public sealed class LoginRequest
{
    [Required]
    [EmailAddress]
    [StringLength(120)]
    public string Email { get; init; } = string.Empty;

    [Required]
    [StringLength(100, MinimumLength = 8)]
    public string Password { get; init; } = string.Empty;
}
