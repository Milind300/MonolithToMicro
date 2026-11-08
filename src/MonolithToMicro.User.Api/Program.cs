using Microsoft.EntityFrameworkCore;
using MonolithToMicro.User.Api.Data;
using MonolithToMicro.User.Api.Repositories;
using MonolithToMicro.User.Api.Services;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHealthChecks();
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("ECommerceDb"), sql =>
        sql.MigrationsHistoryTable("__EFMigrationsHistory", "users")));
builder.Services.AddScoped<ICustomerProfileRepository, CustomerProfileRepository>();
builder.Services.AddScoped<ICustomerProfileService, CustomerProfileService>();
var app = builder.Build();
if (app.Environment.IsDevelopment()) { app.UseSwagger(); app.UseSwaggerUI(); }
app.MapHealthChecks("/health");
app.MapControllers();
app.Run();
