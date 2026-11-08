using MonolithToMicro.Product.Api.Models;
using MonolithToMicro.Product.Api.Repositories;
namespace MonolithToMicro.Product.Api.Services;
public sealed class InventoryItemService(IInventoryItemRepository repository) : IInventoryItemService
{
    public Task<IReadOnlyList<InventoryItem>> GetAllAsync(CancellationToken ct) => repository.GetAllAsync(ct);
    public Task<InventoryItem?> GetAsync(Guid id, CancellationToken ct) => repository.GetByIdAsync(id, ct);
    public async Task<InventoryItem> CreateAsync(InventoryItem model, CancellationToken ct)
    {
        model.Id = Guid.NewGuid();


        await repository.AddAsync(model, ct);
        return model;
    }
    public Task<bool> UpdateAsync(Guid id, InventoryItem model, CancellationToken ct)
    {
        model.Id = id;
        return repository.UpdateAsync(model, ct);
    }
    public Task<bool> DeleteAsync(Guid id, CancellationToken ct) => repository.DeleteAsync(id, ct);
}
