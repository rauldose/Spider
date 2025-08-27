using Spider.Communication.Infrastructure.Extensions;
using Spider.Drivers.Core.Extensions;
using Microsoft.OpenApi.Models;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();

// Add Communication infrastructure and application services
builder.Services.AddCommunicationInfrastructure(builder.Configuration);
builder.Services.AddCommunicationApplication();

// Add Driver infrastructure
builder.Services.AddDriverInfrastructure(builder.Configuration);

// Add Swagger/OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Spider Communication API",
        Version = "v1",
        Description = "API for managing communication links, channels, and data points in the Spider IoT platform",
        Contact = new OpenApiContact
        {
            Name = "Spider Development Team",
            Email = "dev@spider-iot.com"
        }
    });

    // Include XML comments
    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
    {
        c.IncludeXmlComments(xmlPath);
    }
});

// Add health checks
builder.Services.AddHealthChecks();

// Add CORS
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// Add logging
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Spider Communication API v1");
        c.RoutePrefix = "swagger";
    });
}

app.UseHttpsRedirection();
app.UseCors();
app.UseAuthorization();

app.MapControllers();
app.MapHealthChecks("/health");

// Ensure database is created (for development)
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<Spider.Communication.Infrastructure.Persistence.CommunicationDbContext>();
    await context.Database.EnsureCreatedAsync();
}

app.Run();