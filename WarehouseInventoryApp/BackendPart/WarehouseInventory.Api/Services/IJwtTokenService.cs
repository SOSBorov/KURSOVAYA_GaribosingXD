using WarehouseInventory.Api.Dtos;
using WarehouseInventory.Api.Models;

namespace WarehouseInventory.Api.Services;

public interface IJwtTokenService
{
    AuthResponse CreateToken(ApplicationUser user);
}
