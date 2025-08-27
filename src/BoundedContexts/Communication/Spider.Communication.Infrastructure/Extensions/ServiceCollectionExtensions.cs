using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Spider.Communication.Application.Interfaces;
using Spider.Communication.Infrastructure.Persistence;
using Spider.Communication.Infrastructure.Repositories;
using Spider.Core.SharedKernel.Abstractions;
using Spider.Core.Application.Interfaces;
using FluentValidation;
using Spider.Communication.Application.Validators;
using Spider.Communication.Application.Mappings;

namespace Spider.Communication.Infrastructure.Extensions;

/// <summary>
/// Dependency injection extensions for Communication Infrastructure
/// </summary>
public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddCommunicationInfrastructure(
        this IServiceCollection services, 
        IConfiguration configuration)
    {
        // Add DbContext
        services.AddDbContext<CommunicationDbContext>(options =>
        {
            var connectionString = configuration.GetConnectionString("DefaultConnection");
            if (string.IsNullOrEmpty(connectionString))
            {
                // Use In-Memory database for development/testing
                options.UseInMemoryDatabase("CommunicationDB");
            }
            else
            {
                options.UseSqlServer(connectionString);
            }
        });

        // Add repositories
        services.AddScoped<ILinkRepository, LinkRepository>();
        services.AddScoped<IChannelRepository, ChannelRepository>();
        services.AddScoped<IDataPointRepository, DataPointRepository>();

        // Add UnitOfWork
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        // Add validation
        services.AddValidatorsFromAssemblyContaining<CreateLinkCommandValidator>();

        // Add AutoMapper
        services.AddCommunicationMappings();

        return services;
    }

    public static IServiceCollection AddCommunicationApplication(this IServiceCollection services)
    {
        // Add MediatR
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(
            typeof(Spider.Communication.Application.Commands.CreateLinkCommand).Assembly));

        return services;
    }
}

/// <summary>
/// Unit of Work implementation for Communication context
/// </summary>
public class UnitOfWork : IUnitOfWork
{
    private readonly CommunicationDbContext _context;

    public UnitOfWork(CommunicationDbContext context)
    {
        _context = context;
    }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await _context.SaveChangesAsync(cancellationToken);
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}

/// <summary>
/// PagedResult implementation
/// </summary>
public class PagedResult<T> : IPagedResult<T>
{
    public PagedResult(List<T> items, int totalCount, int page, int pageSize)
    {
        Items = items;
        TotalCount = totalCount;
        Page = page;
        PageSize = pageSize;
        TotalPages = (int)Math.Ceiling((double)totalCount / pageSize);
        HasPreviousPage = page > 1;
        HasNextPage = page < TotalPages;
    }

    public List<T> Items { get; }
    public int TotalCount { get; }
    public int Page { get; }
    public int PageSize { get; }
    public int TotalPages { get; }
    public bool HasPreviousPage { get; }
    public bool HasNextPage { get; }
}

/// <summary>
/// Result implementation
/// </summary>
public class Result<T> : IResult<T>
{
    protected Result(bool isSuccess, T? value, string? error)
    {
        IsSuccess = isSuccess;
        Value = value;
        Error = error;
    }

    public bool IsSuccess { get; }
    public bool IsFailure => !IsSuccess;
    public T? Value { get; }
    public string? Error { get; }

    public static IResult<T> Success(T value) => new Result<T>(true, value, null);
    public static IResult<T> Failure(string error) => new Result<T>(false, default, error);
}