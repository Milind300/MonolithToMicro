using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using MonolithToMicro.Order.Api.Data;
using MonolithToMicro.Order.Api.Models;
namespace MonolithToMicro.Order.Api.Repositories;
public sealed class CustomerOrderRepository(AppDbContext db) : ICustomerOrderRepository
{
    public async Task<IReadOnlyList<CustomerOrder>> GetAllAsync(CancellationToken ct) =>
        await db.CustomerOrders.AsNoTracking().ToListAsync(ct);
    public Task<CustomerOrder?> GetByIdAsync(Guid id, CancellationToken ct) =>
        db.CustomerOrders.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id, ct);
    public async Task AddAsync(CustomerOrder model, CancellationToken ct)
    {
        // Both order and outbox are committed within the same SaveChanges transaction.
        await db.CustomerOrders.AddAsync(model, ct);
        await db.OutboxMessages.AddAsync(new OutboxMessage {
            EventType = "OrderPlaced.v1",
            Payload = JsonSerializer.Serialize(new { OrderId = model.Id, model.CustomerId, model.TotalAmount }),
            OccurredAtUtc = DateTime.UtcNow
        }, ct);
        await db.SaveChangesAsync(ct);
    }
    public async Task<bool> UpdateAsync(CustomerOrder model, CancellationToken ct)
    {
        var existing = await db.CustomerOrders.FindAsync([model.Id], ct);
        if (existing is null) return false;
        db.Entry(existing).CurrentValues.SetValues(model);
        await db.SaveChangesAsync(ct);
        return true;
    }
    public async Task<bool> DeleteAsync(Guid id, CancellationToken ct)
    {
        var existing = await db.CustomerOrders.FindAsync([id], ct);
        if (existing is null) return false;
        db.CustomerOrders.Remove(existing);
        await db.SaveChangesAsync(ct);
        return true;
    }
}
