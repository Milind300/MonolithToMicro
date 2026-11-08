using Microsoft.EntityFrameworkCore;
using MonolithToMicro.Product.Api.Data;
using MonolithToMicro.Product.Api.Models;
namespace MonolithToMicro.Product.Api.Repositories;
public sealed class InventoryItemRepository(AppDbContext db) : IInventoryItemRepository
{
    public async Task<IReadOnlyList<InventoryItem>> GetAllAsync(CancellationToken ct) =>
        await db.InventoryItems.AsNoTracking().ToListAsync(ct);
    public Task<InventoryItem?> GetByIdAsync(Guid id, CancellationToken ct) =>
        db.InventoryItems.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id, ct);
    public async Task AddAsync(InventoryItem model, CancellationToken ct)
    {
        await db.InventoryItems.AddAsync(model, ct);
        await db.SaveChangesAsync(ct);
    }
    public async Task<bool> UpdateAsync(InventoryItem model, CancellationToken ct)
    {
        var existing = await db.InventoryItems.FindAsync([model.Id], ct);
        if (existing is null) return false;
        db.Entry(existing).CurrentValues.SetValues(model);
        await db.SaveChangesAsync(ct);
        return true;
    }
    public async Task<bool> DeleteAsync(Guid id, CancellationToken ct)
    {
        var existing = await db.InventoryItems.FindAsync([id], ct);
        if (existing is null) return false;
        db.InventoryItems.Remove(existing);
        await db.SaveChangesAsync(ct);
        return true;
    }
}
