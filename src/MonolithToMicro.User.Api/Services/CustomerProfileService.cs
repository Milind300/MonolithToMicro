using MonolithToMicro.User.Api.Models;
using MonolithToMicro.User.Api.Repositories;
namespace MonolithToMicro.User.Api.Services;
public sealed class CustomerProfileService(ICustomerProfileRepository repository) : ICustomerProfileService
{
    public Task<IReadOnlyList<CustomerProfile>> GetAllAsync(CancellationToken ct) => repository.GetAllAsync(ct);
    public Task<CustomerProfile?> GetAsync(Guid id, CancellationToken ct) => repository.GetByIdAsync(id, ct);
    public async Task<CustomerProfile> CreateAsync(CustomerProfile model, CancellationToken ct)
    {
        model.Id = Guid.NewGuid();


        await repository.AddAsync(model, ct);
        return model;
    }
    public Task<bool> UpdateAsync(Guid id, CustomerProfile model, CancellationToken ct)
    {
        model.Id = id;
        return repository.UpdateAsync(model, ct);
    }
    public Task<bool> DeleteAsync(Guid id, CancellationToken ct) => repository.DeleteAsync(id, ct);
}
