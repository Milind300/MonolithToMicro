using Microsoft.EntityFrameworkCore;
using MonolithToMicro.Catalog.Api.Models;
namespace MonolithToMicro.Catalog.Api.Data;
public sealed class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<CatalogItem> CatalogItems => Set<CatalogItem>();
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("catalog");
        var entity = modelBuilder.Entity<CatalogItem>();
        entity.ToTable("CatalogItems", "catalog");
        entity.HasKey(x => x.Id);
        entity.Property(x => x.Name).HasMaxLength(200).IsRequired();
        entity.Property(x => x.Description).HasMaxLength(200).IsRequired();
        entity.Property(x => x.Category).HasMaxLength(200).IsRequired();
        entity.Property(x => x.Price).HasPrecision(18, 2);
    }
}
