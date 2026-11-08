using MonolithToMicro.Catalog.Api.Models;
namespace MonolithToMicro.Catalog.Api.Repositories;
public interface ICatalogItemRepository
{
    Task<IReadOnlyList<CatalogItem>> GetAllAsync(CancellationToken ct);
    Task<CatalogItem?> GetByIdAsync(Guid id, CancellationToken ct);
    Task AddAsync(CatalogItem model, CancellationToken ct);
    Task<bool> UpdateAsync(CatalogItem model, CancellationToken ct);
    Task<bool> DeleteAsync(Guid id, CancellationToken ct);
}
