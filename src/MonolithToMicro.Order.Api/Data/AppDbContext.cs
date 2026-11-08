using Microsoft.EntityFrameworkCore;
using MonolithToMicro.Order.Api.Models;
namespace MonolithToMicro.Order.Api.Data;
public sealed class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();
    public DbSet<CustomerOrder> CustomerOrders => Set<CustomerOrder>();
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("orders");
        modelBuilder.Entity<OutboxMessage>(b => {
            b.ToTable("OutboxMessages", "orders");
            b.HasKey(x => x.Id);
            b.Property(x => x.EventType).HasMaxLength(200).IsRequired();
            b.Property(x => x.Payload).IsRequired();
            b.HasIndex(x => new { x.PublishedAtUtc, x.OccurredAtUtc });
        });
        var entity = modelBuilder.Entity<CustomerOrder>();
        entity.ToTable("CustomerOrders", "orders");
        entity.HasKey(x => x.Id);
        entity.Property(x => x.Status).HasMaxLength(200).IsRequired();
        entity.Property(x => x.TotalAmount).HasPrecision(18, 2);


    }
}
