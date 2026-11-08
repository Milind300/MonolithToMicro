using Microsoft.EntityFrameworkCore;
using MonolithToMicro.Order.Api.Data;


namespace MonolithToMicro.Order.Api.Repositories;

public sealed record PendingOutboxPreview(
    Guid Id,
    string EventType,
    DateTime OccurredAtUtc);

public interface IOutboxPreviewRepository
{
    Task<IReadOnlyList<PendingOutboxPreview>> GetPendingAsync(
        CancellationToken ct);
}

public sealed class OutboxPreviewRepository(AppDbContext db)
    : IOutboxPreviewRepository
{
    public async Task<IReadOnlyList<PendingOutboxPreview>> GetPendingAsync(
        CancellationToken ct) =>
        await db.OutboxMessages
            .AsNoTracking()
            .Where(message => message.PublishedAtUtc == null)
            .OrderBy(message => message.OccurredAtUtc)
            .ThenBy(message => message.Id)
            .Take(20)
            .Select(message => new PendingOutboxPreview(
                message.Id,
                message.EventType,
                message.OccurredAtUtc))
            .ToListAsync(ct);
}