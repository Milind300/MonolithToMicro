using Microsoft.AspNetCore.Mvc;
using MonolithToMicro.Product.Api.Models;
using MonolithToMicro.Product.Api.Services;
namespace MonolithToMicro.Product.Api.Controllers;
[ApiController]
[Route("api/product")]
public sealed class InventoryItemsController(IInventoryItemService service) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<InventoryItem>>> GetAll(CancellationToken ct) => Ok(await service.GetAllAsync(ct));
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<InventoryItem>> Get(Guid id, CancellationToken ct)
    {
        var item = await service.GetAsync(id, ct);
        return item is null ? NotFound() : Ok(item);
    }
    [HttpPost]
    public async Task<ActionResult<InventoryItem>> Post([FromBody] InventoryItem model, CancellationToken ct)
    {
        var saved = await service.CreateAsync(model, ct);
        return CreatedAtAction(nameof(Get), new { id = saved.Id }, saved);
    }
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Put(Guid id, [FromBody] InventoryItem model, CancellationToken ct) =>
        await service.UpdateAsync(id, model, ct) ? NoContent() : NotFound();
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct) =>
        await service.DeleteAsync(id, ct) ? NoContent() : NotFound();
}
