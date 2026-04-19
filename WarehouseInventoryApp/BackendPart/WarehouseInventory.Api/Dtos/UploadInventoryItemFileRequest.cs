using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace WarehouseInventory.Api.Dtos;

public sealed class UploadInventoryItemFileRequest
{
    [Required]
    public IFormFile File { get; init; } = null!;
}
