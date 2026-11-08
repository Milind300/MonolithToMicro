using MonolithToMicro.Order.Api.Repositories;

namespace MonolithToMicro.Order.Api.Services;

public interface IOutboxPreviewService
{
    Task<IReadOnlyList<PendingOutboxPreview>> GetPendingAsync(
        CancellationToken ct);
}

public sealed class OutboxPreviewService(IOutboxPreviewRepository repository)
    : IOutboxPreviewService
{
    public Task<IReadOnlyList<PendingOutboxPreview>> GetPendingAsync(
        CancellationToken ct) =>
        repository.GetPendingAsync(ct);
}