using Microsoft.AspNetCore.Mvc;
using MonolithToMicro.Order.Api.Repositories;
using MonolithToMicro.Order.Api.Services;

namespace MonolithToMicro.Order.Api.Controllers;

[ApiController]
[Route("api/outbox/preview")]
public sealed class OutboxPreviewController(
    IOutboxPreviewService service,
    IWebHostEnvironment environment) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<PendingOutboxPreview>>> Get(
        CancellationToken ct)
    {
        if (!environment.IsDevelopment())
        {
            return NotFound();
        }

        var messages = await service.GetPendingAsync(ct);
        return Ok(messages);
    }
}