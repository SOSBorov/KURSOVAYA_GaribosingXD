using Microsoft.EntityFrameworkCore;
using WarehouseInventory.Api.Models;

namespace WarehouseInventory.Api.Data;

public sealed class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : DbContext(options)
{
    public DbSet<InventoryItem> InventoryItems => Set<InventoryItem>();
    public DbSet<ApplicationUser> Users => Set<ApplicationUser>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        var inventoryItem = modelBuilder.Entity<InventoryItem>();
        var user = modelBuilder.Entity<ApplicationUser>();

        inventoryItem.ToTable("InventoryItems");
        inventoryItem.HasKey(item => item.Id);
        inventoryItem.HasIndex(item => item.Sku).IsUnique();
        inventoryItem.Property(item => item.Name).IsRequired().HasMaxLength(120);
        inventoryItem.Property(item => item.Sku).IsRequired().HasMaxLength(50);
        inventoryItem.Property(item => item.Category).IsRequired().HasMaxLength(80);
        inventoryItem.Property(item => item.UnitOfMeasure).IsRequired().HasMaxLength(20);
        inventoryItem.Property(item => item.WarehouseLocation).IsRequired().HasMaxLength(80);
        inventoryItem.Property(item => item.UnitPrice).HasPrecision(18, 2);

        user.ToTable("Users");
        user.HasKey(currentUser => currentUser.Id);
        user.HasIndex(currentUser => currentUser.UserName).IsUnique();
        user.HasIndex(currentUser => currentUser.Email).IsUnique();
        user.Property(currentUser => currentUser.UserName).IsRequired().HasMaxLength(50);
        user.Property(currentUser => currentUser.Email).IsRequired().HasMaxLength(120);
        user.Property(currentUser => currentUser.PasswordHash).IsRequired();
    }
}
