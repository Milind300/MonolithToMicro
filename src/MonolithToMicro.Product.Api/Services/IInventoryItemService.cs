using MonolithToMicro.Product.Api.Models;
namespace MonolithToMicro.Product.Api.Services;
public interface IInventoryItemService
{
    Task<IReadOnlyList<InventoryItem>> GetAllAsync(CancellationToken ct);
    Task<InventoryItem?> GetAsync(Guid id, CancellationToken ct);
    Task<InventoryItem> CreateAsync(InventoryItem model, CancellationToken ct);
    Task<bool> UpdateAsync(Guid id, InventoryItem model, CancellationToken ct);
    Task<bool> DeleteAsync(Guid id, CancellationToken ct);
}
