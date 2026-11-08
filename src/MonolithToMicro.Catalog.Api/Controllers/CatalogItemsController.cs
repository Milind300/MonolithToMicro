using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MonolithToMicro.Catalog.Api.Contracts;
using MonolithToMicro.Catalog.Api.Models;
using MonolithToMicro.Catalog.Api.Services;

namespace MonolithToMicro.Catalog.Api.Controllers;

[ApiController]
[Route("api/catalog")]
public sealed class CatalogItemsController(ICatalogItemService service)
    : ControllerBase
{
    [Authorize(Policy = "CatalogRead")]
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<CatalogItem>>> GetAll(
        CancellationToken ct) =>
        Ok(await service.GetAllAsync(ct));

    [Authorize(Policy = "CatalogRead")]
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<CatalogItem>> Get(
        Guid id,
        CancellationToken ct)
    {
        var item = await service.GetAsync(id, ct);
        return item is null ? NotFound() : Ok(item);
    }

    [Authorize(Policy = "CatalogWrite")]
    [HttpPost]
    public async Task<ActionResult<CatalogItem>> Post(
        [FromBody] SaveCatalogItemRequest request,
        CancellationToken ct)
    {
        var saved = await service.CreateAsync(ToModel(request), ct);

        return CreatedAtAction(
            nameof(Get),
            new { id = saved.Id },
            saved);
    }

    [Authorize(Policy = "CatalogWrite")]
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Put(
        Guid id,
        [FromBody] SaveCatalogItemRequest request,
        CancellationToken ct) =>
        await service.UpdateAsync(id, ToModel(request), ct)
            ? NoContent()
            : NotFound();

    [Authorize(Policy = "CatalogWrite")]
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(
        Guid id,
        CancellationToken ct) =>
        await service.DeleteAsync(id, ct)
            ? NoContent()
            : NotFound();

    private static CatalogItem ToModel(SaveCatalogItemRequest request) =>
        new()
        {
            Name = request.Name,
            Description = request.Description,
            Category = request.Category,
            Price = request.Price,
            IsActive = request.IsActive
        };
}