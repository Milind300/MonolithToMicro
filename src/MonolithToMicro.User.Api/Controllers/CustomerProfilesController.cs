using Microsoft.AspNetCore.Mvc;
using MonolithToMicro.User.Api.Models;
using MonolithToMicro.User.Api.Services;
namespace MonolithToMicro.User.Api.Controllers;
[ApiController]
[Route("api/user")]
public sealed class CustomerProfilesController(ICustomerProfileService service) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<CustomerProfile>>> GetAll(CancellationToken ct) => Ok(await service.GetAllAsync(ct));
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<CustomerProfile>> Get(Guid id, CancellationToken ct)
    {
        var item = await service.GetAsync(id, ct);
        return item is null ? NotFound() : Ok(item);
    }
    [HttpPost]
    public async Task<ActionResult<CustomerProfile>> Post([FromBody] CustomerProfile model, CancellationToken ct)
    {
        var saved = await service.CreateAsync(model, ct);
        return CreatedAtAction(nameof(Get), new { id = saved.Id }, saved);
    }
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Put(Guid id, [FromBody] CustomerProfile model, CancellationToken ct) =>
        await service.UpdateAsync(id, model, ct) ? NoContent() : NotFound();
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct) =>
        await service.DeleteAsync(id, ct) ? NoContent() : NotFound();
}
