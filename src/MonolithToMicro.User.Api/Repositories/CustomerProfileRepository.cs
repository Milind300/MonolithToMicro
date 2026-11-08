using Microsoft.EntityFrameworkCore;
using MonolithToMicro.User.Api.Data;
using MonolithToMicro.User.Api.Models;
namespace MonolithToMicro.User.Api.Repositories;
public sealed class CustomerProfileRepository(AppDbContext db) : ICustomerProfileRepository
{
    public async Task<IReadOnlyList<CustomerProfile>> GetAllAsync(CancellationToken ct) =>
        await db.CustomerProfiles.AsNoTracking().ToListAsync(ct);
    public Task<CustomerProfile?> GetByIdAsync(Guid id, CancellationToken ct) =>
        db.CustomerProfiles.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id, ct);
    public async Task AddAsync(CustomerProfile model, CancellationToken ct)
    {
        await db.CustomerProfiles.AddAsync(model, ct);
        await db.SaveChangesAsync(ct);
    }
    public async Task<bool> UpdateAsync(CustomerProfile model, CancellationToken ct)
    {
        var existing = await db.CustomerProfiles.FindAsync([model.Id], ct);
        if (existing is null) return false;
        db.Entry(existing).CurrentValues.SetValues(model);
        await db.SaveChangesAsync(ct);
        return true;
    }
    public async Task<bool> DeleteAsync(Guid id, CancellationToken ct)
    {
        var existing = await db.CustomerProfiles.FindAsync([id], ct);
        if (existing is null) return false;
        db.CustomerProfiles.Remove(existing);
        await db.SaveChangesAsync(ct);
        return true;
    }
}
