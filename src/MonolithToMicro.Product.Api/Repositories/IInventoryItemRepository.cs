using MonolithToMicro.Product.Api.Models;
namespace MonolithToMicro.Product.Api.Repositories;
public interface IInventoryItemRepository
{
    Task<IReadOnlyList<InventoryItem>> GetAllAsync(CancellationToken ct);
    Task<InventoryItem?> GetByIdAsync(Guid id, CancellationToken ct);
    Task AddAsync(InventoryItem model, CancellationToken ct);
    Task<bool> UpdateAsync(InventoryItem model, CancellationToken ct);
    Task<bool> DeleteAsync(Guid id, CancellationToken ct);
}
