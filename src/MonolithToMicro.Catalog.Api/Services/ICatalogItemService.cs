using MonolithToMicro.Catalog.Api.Models;
namespace MonolithToMicro.Catalog.Api.Services;
public interface ICatalogItemService
{
    Task<IReadOnlyList<CatalogItem>> GetAllAsync(CancellationToken ct);
    Task<CatalogItem?> GetAsync(Guid id, CancellationToken ct);
    Task<CatalogItem> CreateAsync(CatalogItem model, CancellationToken ct);
    Task<bool> UpdateAsync(Guid id, CatalogItem model, CancellationToken ct);
    Task<bool> DeleteAsync(Guid id, CancellationToken ct);
}
