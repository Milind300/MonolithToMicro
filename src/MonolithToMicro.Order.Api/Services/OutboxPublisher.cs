using Azure.Messaging.ServiceBus;
using Microsoft.EntityFrameworkCore;
using MonolithToMicro.Order.Api.Data;

namespace MonolithToMicro.Order.Api.Services;

public sealed class OutboxPublisher(
    IServiceScopeFactory scopeFactory,
    ServiceBusClient serviceBusClient,
    IConfiguration configuration,
    ILogger<OutboxPublisher> logger) : BackgroundService
{
    private const int BatchSize = 20;
    private static readonly TimeSpan PollInterval = TimeSpan.FromSeconds(10);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var topicName = configuration["ServiceBus:TopicName"];

        if (string.IsNullOrWhiteSpace(topicName))
        {
            logger.LogError(
                "Outbox publisher stopped: ServiceBus:TopicName is missing.");
            return;
        }

        await using var sender = serviceBusClient.CreateSender(topicName);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await PublishBatchAsync(sender, stoppingToken);
            }
            catch (OperationCanceledException)
                when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception exception)
            {
                logger.LogError(
                    exception,
                    "Outbox publishing failed. Pending messages will be retried.");
            }

            try
            {
                await Task.Delay(PollInterval, stoppingToken);
            }
            catch (OperationCanceledException)
                when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
        }
    }

    private async Task PublishBatchAsync(
        ServiceBusSender sender,
        CancellationToken ct)
    {
        await using var scope = scopeFactory.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var messages = await db.OutboxMessages
            .Where(message => message.PublishedAtUtc == null)
            .OrderBy(message => message.OccurredAtUtc)
            .ThenBy(message => message.Id)
            .Take(BatchSize)
            .ToListAsync(ct);

        foreach (var outboxMessage in messages)
        {
            var busMessage = new ServiceBusMessage(outboxMessage.Payload)
            {
                MessageId = outboxMessage.Id.ToString(),
                Subject = outboxMessage.EventType,
                ContentType = "application/json"
            };

            // Only mark as published after Service Bus accepts the message.
            await sender.SendMessageAsync(busMessage, ct);

            outboxMessage.PublishedAtUtc = DateTime.UtcNow;
            await db.SaveChangesAsync(ct);

            logger.LogInformation(
                "Published outbox message {OutboxMessageId} of type {EventType}.",
                outboxMessage.Id,
                outboxMessage.EventType);
        }
    }
}