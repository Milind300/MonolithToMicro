using MonolithToMicro.User.Api.Models;
namespace MonolithToMicro.User.Api.Repositories;
public interface ICustomerProfileRepository
{
    Task<IReadOnlyList<CustomerProfile>> GetAllAsync(CancellationToken ct);
    Task<CustomerProfile?> GetByIdAsync(Guid id, CancellationToken ct);
    Task AddAsync(CustomerProfile model, CancellationToken ct);
    Task<bool> UpdateAsync(CustomerProfile model, CancellationToken ct);
    Task<bool> DeleteAsync(Guid id, CancellationToken ct);
}
