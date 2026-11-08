using Microsoft.EntityFrameworkCore;
using MonolithToMicro.User.Api.Models;
namespace MonolithToMicro.User.Api.Data;
public sealed class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<CustomerProfile> CustomerProfiles => Set<CustomerProfile>();
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("users");
        var entity = modelBuilder.Entity<CustomerProfile>();
        entity.ToTable("CustomerProfiles", "users");
        entity.HasKey(x => x.Id);
        entity.Property(x => x.DisplayName).HasMaxLength(200).IsRequired();
        entity.Property(x => x.Email).HasMaxLength(200).IsRequired();
        entity.Property(x => x.City).HasMaxLength(200).IsRequired();

        entity.HasIndex(x => x.Email).IsUnique();

    }
}
