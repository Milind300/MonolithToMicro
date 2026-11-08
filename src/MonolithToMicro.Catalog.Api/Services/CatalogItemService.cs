using MonolithToMicro.Catalog.Api.Models;
using MonolithToMicro.Catalog.Api.Repositories;
namespace MonolithToMicro.Catalog.Api.Services;
public sealed class CatalogItemService(ICatalogItemRepository repository) : ICatalogItemService
{
    public Task<IReadOnlyList<CatalogItem>> GetAllAsync(CancellationToken ct) => repository.GetAllAsync(ct);
    public Task<CatalogItem?> GetAsync(Guid id, CancellationToken ct) => repository.GetByIdAsync(id, ct);
    public async Task<CatalogItem> CreateAsync(CatalogItem model, CancellationToken ct)
    {
        model.Id = Guid.NewGuid();


        await repository.AddAsync(model, ct);
        return model;
    }
    public Task<bool> UpdateAsync(Guid id, CatalogItem model, CancellationToken ct)
    {
        model.Id = id;
        return repository.UpdateAsync(model, ct);
    }
    public Task<bool> DeleteAsync(Guid id, CancellationToken ct) => repository.DeleteAsync(id, ct);
}
