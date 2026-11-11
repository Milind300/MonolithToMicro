
using System;
using System.Text.Json;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace MonolithToMicro.OrderProcessing.Functions;

public sealed class ProcessOrderFromQueue
{
    private readonly ILogger<ProcessOrderFromQueue> _logger;

    public ProcessOrderFromQueue(ILogger<ProcessOrderFromQueue> logger)
    {
        _logger = logger;
    }

    [Function("ProcessOrderFromQueue")]
    public void Run(
        [ServiceBusTrigger("order-placed", Connection = "OrderServiceBusConnection")]
        string message)
    {
        using var json = JsonDocument.Parse(message);
        var root = json.RootElement;

        // Match the actual JSON payload produced by Order API.
        if (!root.TryGetProperty("OrderId", out var orderIdElement) ||
            orderIdElement.ValueKind != JsonValueKind.String ||
            !Guid.TryParse(orderIdElement.GetString(), out var orderId) ||
            orderId == Guid.Empty)
        {
            throw new InvalidOperationException(
                "Invalid OrderPlaced.v1 event: OrderId is missing or invalid.");
        }

        _logger.LogInformation(
            "OrderPlaced.v1 message consumed successfully. OrderId={OrderId}",
            orderId);
    }
}
