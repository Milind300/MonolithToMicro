
using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using MonolithToMicro.Order.Api.Models;
using MonolithToMicro.Order.Api.Services;

namespace MonolithToMicro.Order.Api.Controllers;

[ApiController]
[Route("api/order")]
public sealed class CustomerOrdersController(
    ICustomerOrderService service) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(
        typeof(IReadOnlyList<CustomerOrder>),
        StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<CustomerOrder>>> GetAll(
        CancellationToken ct)
    {
        var orders = await service.GetAllAsync(ct);

        return Ok(orders);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(
        typeof(CustomerOrder),
        StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CustomerOrder>> Get(
        Guid id,
        CancellationToken ct)
    {
        var order = await service.GetAsync(id, ct);

        return order is null ? NotFound() : Ok(order);
    }

    [HttpPost]
    [ProducesResponseType(
        typeof(CustomerOrder),
        StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status502BadGateway)]
    [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
    [ProducesResponseType(StatusCodes.Status504GatewayTimeout)]
    public async Task<ActionResult<CustomerOrder>> Post(
        [FromBody] CreateOrderRequest request,
        CancellationToken ct)
    {
        try
        {
            var saved = await service.CreateFromRequestAsync(
                request,
                ct);

            return CreatedAtAction(
                nameof(Get),
                new { id = saved.Id },
                saved);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { error = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { error = ex.Message });
        }
        catch (HttpRequestException ex)
            when (ex.StatusCode is not null &&
                  ex.StatusCode != HttpStatusCode.NotFound)
        {
            return StatusCode(
                StatusCodes.Status502BadGateway,
                new { error = "Product API returned an unsuccessful response." });
        }
        catch (HttpRequestException)
        {
            return StatusCode(
                StatusCodes.Status503ServiceUnavailable,
                new { error = "Product API is unavailable." });
        }
        catch (OperationCanceledException)
            when (!ct.IsCancellationRequested)
        {
            return StatusCode(
                StatusCodes.Status504GatewayTimeout,
                new { error = "Product API request timed out." });
        }
        catch (JsonException)
        {
            return StatusCode(
                StatusCodes.Status502BadGateway,
                new { error = "Product API returned invalid JSON." });
        }
        catch (InvalidDataException)
        {
            return StatusCode(
                StatusCodes.Status502BadGateway,
                new { error = "Product API returned invalid product data." });
        }
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Put(
        Guid id,
        [FromBody] UpdateOrderStatusRequest request,
        CancellationToken ct)
    {
        try
        {
            var updated = await service.UpdateStatusAsync(
                id,
                request,
                ct);

            return updated ? NoContent() : NotFound();
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(
        Guid id,
        CancellationToken ct)
    {
        var deleted = await service.DeleteAsync(id, ct);

        return deleted ? NoContent() : NotFound();
    }
}
