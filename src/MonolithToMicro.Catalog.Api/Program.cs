using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Web;
using MonolithToMicro.Catalog.Api.Data;
using MonolithToMicro.Catalog.Api.Repositories;
using MonolithToMicro.Catalog.Api.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHealthChecks();

// Microsoft Entra ID JWT validation
builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddMicrosoftIdentityWebApi(
        builder.Configuration.GetSection("AzureAd"));

// Scope-based authorization
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("CatalogRead", policy =>
        policy.RequireAuthenticatedUser()
              .RequireAssertion(context =>
                  context.User.Claims
                      .Where(c =>
                          c.Type == "scp" ||
                          c.Type == "http://schemas.microsoft.com/identity/claims/scope")
                      .SelectMany(c => c.Value.Split(
                          ' ', StringSplitOptions.RemoveEmptyEntries))
                      .Contains("Catalog.Read")));

    options.AddPolicy("CatalogWrite", policy =>
        policy.RequireAuthenticatedUser()
              .RequireAssertion(context =>
                  context.User.Claims
                      .Where(c =>
                          c.Type == "scp" ||
                          c.Type == "http://schemas.microsoft.com/identity/claims/scope")
                      .SelectMany(c => c.Value.Split(
                          ' ', StringSplitOptions.RemoveEmptyEntries))
                      .Contains("Catalog.Write")));
});

// React development origin
builder.Services.AddCors(options =>
{
    options.AddPolicy("ReactClient", policy =>
        policy.WithOrigins("http://localhost:5173")
              .AllowAnyHeader()
              .AllowAnyMethod());
});

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("ECommerceDb"),
        sql => sql.MigrationsHistoryTable(
            "__EFMigrationsHistory", "catalog")));

builder.Services.AddScoped<ICatalogItemRepository, CatalogItemRepository>();
builder.Services.AddScoped<ICatalogItemService, CatalogItemService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("ReactClient");

app.UseAuthentication();
app.UseAuthorization();

app.MapHealthChecks("/health");
app.MapControllers().RequireAuthorization();

app.Run();