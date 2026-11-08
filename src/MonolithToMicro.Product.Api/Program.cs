using Microsoft.EntityFrameworkCore;
using MonolithToMicro.Product.Api.Data;
using MonolithToMicro.Product.Api.Repositories;
using MonolithToMicro.Product.Api.Services;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHealthChecks();
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("ECommerceDb"), sql =>
        sql.MigrationsHistoryTable("__EFMigrationsHistory", "product")));
builder.Services.AddScoped<IInventoryItemRepository, InventoryItemRepository>();
builder.Services.AddScoped<IInventoryItemService, InventoryItemService>();
var app = builder.Build();
if (app.Environment.IsDevelopment()) { app.UseSwagger(); app.UseSwaggerUI(); }
app.MapHealthChecks("/health");
app.MapControllers();
app.Run();
