using Microsoft.EntityFrameworkCore;
using WarehouseInventory.Api.Data;

namespace WarehouseInventory.Tests.Helpers;

internal static class TestDbContextFactory
{
    public static ApplicationDbContext Create(string? databaseName = null)
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName ?? Guid.NewGuid().ToString())
            .Options;

        return new ApplicationDbContext(options);
    }
}
