using Microsoft.EntityFrameworkCore;
using MonolithToMicro.Catalog.Api.Data;
using MonolithToMicro.Catalog.Api.Models;
namespace MonolithToMicro.Catalog.Api.Repositories;
public sealed class CatalogItemRepository(AppDbContext db) : ICatalogItemRepository
{
    public async Task<IReadOnlyList<CatalogItem>> GetAllAsync(CancellationToken ct) =>
        await db.CatalogItems.AsNoTracking().ToListAsync(ct);
    public Task<CatalogItem?> GetByIdAsync(Guid id, CancellationToken ct) =>
        db.CatalogItems.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id, ct);
    public async Task AddAsync(CatalogItem model, CancellationToken ct)
    {
        await db.CatalogItems.AddAsync(model, ct);
        await db.SaveChangesAsync(ct);
    }
    public async Task<bool> UpdateAsync(CatalogItem model, CancellationToken ct)
    {
        var existing = await db.CatalogItems.FindAsync([model.Id], ct);
        if (existing is null) return false;
        db.Entry(existing).CurrentValues.SetValues(model);
        await db.SaveChangesAsync(ct);
        return true;
    }
    public async Task<bool> DeleteAsync(Guid id, CancellationToken ct)
    {
        var existing = await db.CatalogItems.FindAsync([id], ct);
        if (existing is null) return false;
        db.CatalogItems.Remove(existing);
        await db.SaveChangesAsync(ct);
        return true;
    }
}
