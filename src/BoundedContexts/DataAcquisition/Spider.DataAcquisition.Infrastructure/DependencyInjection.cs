using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Spider.Core.SharedKernel.Abstractions;
using Spider.DataAcquisition.Domain.Entities;
using Spider.DataAcquisition.Infrastructure.Data;
using Spider.DataAcquisition.Infrastructure.Repositories;

namespace Spider.DataAcquisition.Infrastructure;

/// <summary>
/// Dependency injection configuration for Data Acquisition Infrastructure
/// </summary>
public static class DependencyInjection
{
    public static IServiceCollection AddDataAcquisitionInfrastructure(
        this IServiceCollection services, 
        string connectionString)
    {
        // Entity Framework
        services.AddDbContext<DataAcquisitionDbContext>(options =>
            options.UseInMemoryDatabase("DataAcquisition")); // Use InMemory for now

        // Repositories
        services.AddScoped<IRepository<DataPoint, Guid>, EfRepository<DataPoint, Guid>>();
        
        // Unit of Work
        services.AddScoped<IUnitOfWork, EfUnitOfWork>();

        return services;
    }
}