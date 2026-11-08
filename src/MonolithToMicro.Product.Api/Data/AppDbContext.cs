using Microsoft.EntityFrameworkCore;
using MonolithToMicro.Product.Api.Models;
namespace MonolithToMicro.Product.Api.Data;
public sealed class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<InventoryItem> InventoryItems => Set<InventoryItem>();
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("product");
        var entity = modelBuilder.Entity<InventoryItem>();
        entity.ToTable("InventoryItems", "product");
        entity.HasKey(x => x.Id);
        entity.Property(x => x.Sku).HasMaxLength(200).IsRequired();
        entity.Property(x => x.Name).HasMaxLength(200).IsRequired();
        entity.Property(x => x.UnitPrice).HasPrecision(18, 2);

        entity.HasIndex(x => x.Sku).IsUnique();
    }
}
