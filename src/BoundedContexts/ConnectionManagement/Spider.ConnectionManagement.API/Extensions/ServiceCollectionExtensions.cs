using MediatR;
using FluentValidation;
using FluentValidation.AspNetCore;
using Spider.ConnectionManagement.Application.Commands;
using Spider.ConnectionManagement.Application.Handlers;
using Spider.ConnectionManagement.Application.Validators;
using Spider.ConnectionManagement.Infrastructure.Configuration;
using Spider.ConnectionManagement.API.Health;
using Spider.ConnectionManagement.API.Middleware;
using Serilog;

namespace Spider.ConnectionManagement.API.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddConnectionManagementApi(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // MediatR
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblyContaining<CreateConnectionCommand>());
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblyContaining<CreateConnectionCommandHandler>());

        // FluentValidation
        services.AddFluentValidationAutoValidation();
        services.AddValidatorsFromAssemblyContaining<CreateConnectionCommandValidator>();

        // Core Application services
        // services.AddCoreApplication(); // Remove this line for now

        // Infrastructure
        services.AddConnectionManagementInfrastructure(configuration);

        // Health Checks
        services.AddHealthChecks()
            .AddCheck<ConnectionManagementHealthCheck>("connection_management");

        // API Documentation
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new() { 
                Title = "Spider ConnectionManagement API", 
                Version = "v1",
                Description = "API for managing IoT device connections and protocol drivers"
            });
            
            var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
            var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
            if (File.Exists(xmlPath))
            {
                c.IncludeXmlComments(xmlPath);
            }
        });

        // CORS
        services.AddCors(options =>
        {
            options.AddPolicy("AllowAll", builder =>
            {
                builder
                    .AllowAnyOrigin()
                    .AllowAnyMethod()
                    .AllowAnyHeader();
            });
        });

        return services;
    }

    public static WebApplication ConfigureConnectionManagementApi(this WebApplication app)
    {
        // Error handling
        app.UseMiddleware<ErrorHandlingMiddleware>();

        // Development tools
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "Spider ConnectionManagement API v1");
                c.RoutePrefix = "swagger";
            });
        }

        // Security and middleware
        app.UseHttpsRedirection();
        app.UseCors("AllowAll");

        // Health checks
        app.UseHealthChecks("/health", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
        {
            ResponseWriter = async (context, report) =>
            {
                context.Response.ContentType = "application/json";
                var result = System.Text.Json.JsonSerializer.Serialize(new
                {
                    status = report.Status.ToString(),
                    checks = report.Entries.Select(e => new
                    {
                        name = e.Key,
                        status = e.Value.Status.ToString(),
                        description = e.Value.Description,
                        data = e.Value.Data
                    })
                });
                await context.Response.WriteAsync(result);
            }
        });

        // API routes
        app.MapControllers();

        return app;
    }
}