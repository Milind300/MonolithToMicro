
using MonolithToMicro.Order.Api.Models;

namespace MonolithToMicro.Order.Api.Services;

public interface ICustomerOrderService
{
    Task<IReadOnlyList<CustomerOrder>> GetAllAsync(
        CancellationToken ct);

    Task<CustomerOrder?> GetAsync(
        Guid id,
        CancellationToken ct);

    Task<CustomerOrder> CreateFromRequestAsync(
        CreateOrderRequest request,
        CancellationToken ct);

    Task<bool> UpdateStatusAsync(
        Guid id,
        UpdateOrderStatusRequest request,
        CancellationToken ct);

    Task<bool> DeleteAsync(
        Guid id,
        CancellationToken ct);
}
