
using MonolithToMicro.Order.Api.Integrations;
using MonolithToMicro.Order.Api.Models;
using MonolithToMicro.Order.Api.Repositories;

namespace MonolithToMicro.Order.Api.Services;

public sealed class CustomerOrderService(
    ICustomerOrderRepository repository,
    ProductApiClient productApiClient) : ICustomerOrderService
{
    public Task<IReadOnlyList<CustomerOrder>> GetAllAsync(
        CancellationToken ct) =>
        repository.GetAllAsync(ct);

    public Task<CustomerOrder?> GetAsync(
        Guid id,
        CancellationToken ct) =>
        repository.GetByIdAsync(id, ct);

    public async Task<CustomerOrder> CreateFromRequestAsync(
        CreateOrderRequest request,
        CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (request.CustomerId == Guid.Empty)
        {
            throw new ArgumentException("CustomerId is required.");
        }

        if (request.ProductId == Guid.Empty)
        {
            throw new ArgumentException("ProductId is required.");
        }

        if (request.Quantity <= 0)
        {
            throw new ArgumentException(
                "Quantity must be greater than zero.");
        }

        var product = await productApiClient.GetByIdAsync(
            request.ProductId,
            ct);

        if (product is null)
        {
            throw new KeyNotFoundException("Product not found.");
        }

        if (product.AvailableQuantity < request.Quantity)
        {
            throw new InvalidOperationException(
                "Insufficient product stock.");
        }

        decimal totalAmount;

        try
        {
            totalAmount = checked(
                product.UnitPrice * request.Quantity);
        }
        catch (OverflowException)
        {
            throw new ArgumentException(
                "Calculated order amount exceeds the supported range.");
        }

        var order = new CustomerOrder
        {
            Id = Guid.NewGuid(),
            CustomerId = request.CustomerId,
            ProductId = request.ProductId,
            Quantity = request.Quantity,
            TotalAmount = totalAmount,
            Status = "Pending",
            CreatedAtUtc = DateTime.UtcNow
        };

        // Existing repository persists the order and
        // OrderPlaced.v1 outbox event in one SaveChangesAsync.
        await repository.AddAsync(order, ct);

        return order;
    }

    public async Task<bool> UpdateStatusAsync(
        Guid id,
        UpdateOrderStatusRequest request,
        CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (id == Guid.Empty)
        {
            throw new ArgumentException("Order ID is required.");
        }

        var validStatuses = new[]
        {
            "Pending",
            "Confirmed",
            "Cancelled"
        };

        if (!validStatuses.Contains(
                request.Status,
                StringComparer.Ordinal))
        {
            throw new ArgumentException(
                "Invalid order status.");
        }

        var existingOrder = await repository.GetByIdAsync(id, ct);

        if (existingOrder is null)
        {
            return false;
        }

        // Only status is changed. Product, quantity,
        // price, customer and creation time remain unchanged.
        existingOrder.Status = request.Status;

        return await repository.UpdateAsync(existingOrder, ct);
    }

    public Task<bool> DeleteAsync(
        Guid id,
        CancellationToken ct) =>
        repository.DeleteAsync(id, ct);
}
