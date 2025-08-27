using Spider.ConnectionManagement.API.Extensions;
using Spider.ConnectionManagement.Infrastructure.Configuration;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .WriteTo.Console()
    .WriteTo.File("logs/connectionmanagement-.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();

builder.Host.UseSerilog();

// Add services
builder.Services.AddControllers();
builder.Services.AddConnectionManagementApi(builder.Configuration);

var app = builder.Build();

// Ensure database is created
await app.Services.EnsureConnectionManagementDatabaseAsync();

// Configure pipeline
app.ConfigureConnectionManagementApi();

// Run application
try
{
    Log.Information("Starting Spider ConnectionManagement API");
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}