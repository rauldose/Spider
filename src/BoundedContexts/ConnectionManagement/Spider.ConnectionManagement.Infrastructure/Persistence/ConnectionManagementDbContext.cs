using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Spider.ConnectionManagement.Domain.Entities;
using Spider.ConnectionManagement.Infrastructure.Persistence.Configurations;
using Spider.Core.SharedKernel.Abstractions;

namespace Spider.ConnectionManagement.Infrastructure.Persistence;

public class ConnectionManagementDbContext : DbContext
{
    private readonly ILogger<ConnectionManagementDbContext> _logger;

    public ConnectionManagementDbContext(
        DbContextOptions<ConnectionManagementDbContext> options,
        ILogger<ConnectionManagementDbContext> logger) : base(options)
    {
        _logger = logger;
    }

    public DbSet<Connection> Connections { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfiguration(new ConnectionConfiguration());

        // Set schema if needed
        modelBuilder.HasDefaultSchema("ConnectionManagement");
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            optionsBuilder.UseInMemoryDatabase("ConnectionManagement");
        }

        optionsBuilder.EnableSensitiveDataLogging(false);
        optionsBuilder.EnableServiceProviderCaching();
        optionsBuilder.EnableDetailedErrors();
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var domainEvents = GetDomainEvents();

        var result = await base.SaveChangesAsync(cancellationToken);

        // Domain events would typically be published here via a domain event dispatcher
        // For now, we'll just log them
        foreach (var domainEvent in domainEvents)
        {
            _logger.LogInformation("Domain event published: {EventType} at {Timestamp}", 
                domainEvent.GetType().Name, DateTime.UtcNow);
        }

        return result;
    }

    private List<IDomainEvent> GetDomainEvents()
    {
        return ChangeTracker.Entries<Connection>()
            .Where(entry => entry.Entity.DomainEvents.Any())
            .SelectMany(entry => entry.Entity.DomainEvents)
            .ToList();
    }
}