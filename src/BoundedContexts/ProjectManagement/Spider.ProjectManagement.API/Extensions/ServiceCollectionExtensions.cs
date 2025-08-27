using Microsoft.EntityFrameworkCore;
using Spider.Core.SharedKernel.Abstractions;
using Spider.ProjectManagement.Application.Handlers;
using Spider.ProjectManagement.Domain.Entities;
using Spider.ProjectManagement.Infrastructure.Data;
using Spider.ProjectManagement.Infrastructure.Repositories;
using FluentValidation;
using MediatR;
using System.Reflection;

namespace Spider.ProjectManagement.API.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddProjectManagementServices(this IServiceCollection services, IConfiguration configuration)
    {
        // Add DbContext
        services.AddDbContext<ProjectManagementDbContext>(options =>
            options.UseInMemoryDatabase("ProjectManagementDb"));

        // Add repositories
        services.AddScoped<IRepository<Project, Guid>, ProjectRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        // Add MediatR
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly()));
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(CreateProjectCommandHandler).Assembly));

        // Add FluentValidation
        services.AddValidatorsFromAssembly(typeof(CreateProjectCommandHandler).Assembly);

        // Add logging behavior
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(Spider.Core.Application.Behaviors.LoggingBehavior<,>));
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(Spider.Core.Application.Behaviors.ValidationBehavior<,>));

        return services;
    }
}