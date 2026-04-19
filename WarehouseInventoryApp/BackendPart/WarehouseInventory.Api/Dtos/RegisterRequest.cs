using System.ComponentModel.DataAnnotations;

namespace WarehouseInventory.Api.Dtos;

public sealed class RegisterRequest
{
    [Required]
    [StringLength(50, MinimumLength = 3)]
    public string UserName { get; init; } = string.Empty;

    [Required]
    [EmailAddress]
    [StringLength(120)]
    public string Email { get; init; } = string.Empty;

    [Required]
    [StringLength(100, MinimumLength = 8)]
    public string Password { get; init; } = string.Empty;
}
