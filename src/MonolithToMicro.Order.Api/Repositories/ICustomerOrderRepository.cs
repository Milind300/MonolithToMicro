using MonolithToMicro.Order.Api.Models;
namespace MonolithToMicro.Order.Api.Repositories;
public interface ICustomerOrderRepository
{
    Task<IReadOnlyList<CustomerOrder>> GetAllAsync(CancellationToken ct);
    Task<CustomerOrder?> GetByIdAsync(Guid id, CancellationToken ct);
    Task AddAsync(CustomerOrder model, CancellationToken ct);
    Task<bool> UpdateAsync(CustomerOrder model, CancellationToken ct);
    Task<bool> DeleteAsync(Guid id, CancellationToken ct);
}
