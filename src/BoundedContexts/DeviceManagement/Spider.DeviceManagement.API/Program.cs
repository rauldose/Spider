using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using MediatR;
using FluentValidation;
using Spider.Core.Application.Behaviors;
using Spider.Core.SharedKernel.Abstractions;
using Spider.DeviceManagement.Domain.Entities;
using Spider.DeviceManagement.Infrastructure.Persistence;
using Spider.DeviceManagement.Infrastructure.Repositories;
using Spider.DeviceManagement.Application.Commands;
using Spider.DeviceManagement.Application.Validators;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// Swagger configuration
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Spider IoT Device Management API",
        Version = "v1",
        Description = "Domain-Driven Design API for Device Management bounded context"
    });
});

// Database configuration
builder.Services.AddDbContext<DeviceManagementDbContext>(options =>
    options.UseInMemoryDatabase("DeviceManagementDb"));

// Repository registration
builder.Services.AddScoped<IRepository<Device, Guid>, DeviceRepository>();
builder.Services.AddScoped<IUnitOfWork, DeviceManagementUnitOfWork>();

// MediatR registration
builder.Services.AddMediatR(cfg =>
{
    cfg.RegisterServicesFromAssembly(typeof(CreateDeviceCommand).Assembly);
    cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));
    cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
});

// FluentValidation
builder.Services.AddValidatorsFromAssembly(typeof(CreateDeviceCommandValidator).Assembly);

// CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy
            .AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader();
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Device Management API v1"));
}

app.UseHttpsRedirection();
app.UseCors("AllowAll");
app.UseAuthorization();
app.MapControllers();

// Initialize database
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<DeviceManagementDbContext>();
    context.Database.EnsureCreated();
}

app.Run();