using MonolithToMicro.User.Api.Models;
namespace MonolithToMicro.User.Api.Services;
public interface ICustomerProfileService
{
    Task<IReadOnlyList<CustomerProfile>> GetAllAsync(CancellationToken ct);
    Task<CustomerProfile?> GetAsync(Guid id, CancellationToken ct);
    Task<CustomerProfile> CreateAsync(CustomerProfile model, CancellationToken ct);
    Task<bool> UpdateAsync(Guid id, CustomerProfile model, CancellationToken ct);
    Task<bool> DeleteAsync(Guid id, CancellationToken ct);
}
