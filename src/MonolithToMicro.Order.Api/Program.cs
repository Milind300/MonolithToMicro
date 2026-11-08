
using Azure.Messaging.ServiceBus;
using Microsoft.EntityFrameworkCore;
using MonolithToMicro.Order.Api.Data;
using MonolithToMicro.Order.Api.Integrations;
using MonolithToMicro.Order.Api.Repositories;
using MonolithToMicro.Order.Api.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHealthChecks();

// SQL Server
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("ECommerceDb"),
        sql => sql.MigrationsHistoryTable(
            "__EFMigrationsHistory",
            "orders")));

// Order repository and service
builder.Services.AddScoped<
    ICustomerOrderRepository,
    CustomerOrderRepository>();

builder.Services.AddScoped<
    ICustomerOrderService,
    CustomerOrderService>();

// Product API address can be overridden using:
// ProductApi__BaseUrl environment variable.
//
// Fallback is permitted only for local Development.
var productApiBaseUrl =
    builder.Configuration["ProductApi:BaseUrl"];

if (string.IsNullOrWhiteSpace(productApiBaseUrl))
{
    if (!builder.Environment.IsDevelopment())
    {
        throw new InvalidOperationException(
            "ProductApi:BaseUrl must be configured.");
    }

    productApiBaseUrl = "http://localhost:5102/";
}

if (!Uri.TryCreate(
        productApiBaseUrl,
        UriKind.Absolute,
        out var productApiUri) ||
    productApiUri.Scheme is not ("http" or "https"))
{
    throw new InvalidOperationException(
        "ProductApi:BaseUrl must be an absolute HTTP(S) URL.");
}

builder.Services.AddHttpClient<ProductApiClient>(client =>
{
    client.BaseAddress = productApiUri;
    client.Timeout = TimeSpan.FromSeconds(10);
});

// Outbox preview
builder.Services.AddScoped<
    IOutboxPreviewRepository,
    OutboxPreviewRepository>();

builder.Services.AddScoped<
    IOutboxPreviewService,
    OutboxPreviewService>();

// Intentionally disabled for the local lab.
// No Azure Service Bus resources are provisioned.
bool enableServiceBusPublisher = false;

if (enableServiceBusPublisher)
{
    var connectionString =
        builder.Configuration["ServiceBus:ConnectionString"];

    if (string.IsNullOrWhiteSpace(connectionString))
    {
        throw new InvalidOperationException(
            "ServiceBus:ConnectionString must be configured.");
    }

    builder.Services.AddSingleton(
        _ => new ServiceBusClient(connectionString));

    builder.Services.AddHostedService<OutboxPublisher>();
}

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapHealthChecks("/health");
app.MapControllers();

app.Run();
