using System.Net;
using System.Text.Json;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;

namespace MonolithToMicro.OrderProcessing.Functions;

public sealed class ProcessOrderHttp
{
    private readonly ILogger<ProcessOrderHttp> _logger;

    public ProcessOrderHttp(ILogger<ProcessOrderHttp> logger)
    {
        _logger = logger;
    }

    [Function("ProcessOrderHttp")]
    public async Task<HttpResponseData> Run(
        [HttpTrigger(AuthorizationLevel.Function, "post", Route = "orders/process")]
        HttpRequestData request)
    {
        OrderPlacedEvent? order;

        try
        {
            order = await JsonSerializer.DeserializeAsync<OrderPlacedEvent>(
                request.Body,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        }
        catch (JsonException)
        {
            return await ErrorResponse(request, "Invalid JSON payload.");
        }

        if (order is null || order.OrderId == Guid.Empty)
        {
            return await ErrorResponse(request, "A valid OrderId is required.");
        }

        _logger.LogInformation("Order event validated. OrderId={OrderId}", order.OrderId);

        var response = request.CreateResponse(HttpStatusCode.Accepted);
        response.Headers.Add("Content-Type", "application/json");
        await response.WriteStringAsync(JsonSerializer.Serialize(new
        {
            orderId = order.OrderId,
            eventType = "OrderPlaced.v1",
            status = "Validated",
            processedAtUtc = DateTimeOffset.UtcNow
        }));
        return response;
    }

    private static async Task<HttpResponseData> ErrorResponse(
        HttpRequestData request, string message)
    {
        var response = request.CreateResponse(HttpStatusCode.BadRequest);
        response.Headers.Add("Content-Type", "application/json");
        await response.WriteStringAsync(JsonSerializer.Serialize(new { error = message }));
        return response;
    }
}

public sealed record OrderPlacedEvent(Guid OrderId);
