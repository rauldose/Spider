using Microsoft.EntityFrameworkCore;
using Spider.Communication.Domain.Entities;
using Spider.Communication.Infrastructure.Persistence.Configurations;
using Spider.Core.SharedKernel.Abstractions;
using Spider.Core.SharedKernel.Base;

namespace Spider.Communication.Infrastructure.Persistence;

/// <summary>
/// Communication DbContext for EF Core
/// </summary>
public class CommunicationDbContext : DbContext
{
    public CommunicationDbContext(DbContextOptions<CommunicationDbContext> options) : base(options)
    {
    }

    public DbSet<Link> Links => Set<Link>();
    public DbSet<Channel> Channels => Set<Channel>();
    public DbSet<DataPoint> DataPoints => Set<DataPoint>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Apply configurations
        modelBuilder.ApplyConfiguration(new LinkConfiguration());
        modelBuilder.ApplyConfiguration(new ChannelConfiguration());
        modelBuilder.ApplyConfiguration(new DataPointConfiguration());

        // Configure schema
        modelBuilder.HasDefaultSchema("Communication");
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        // Dispatch domain events before saving
        await DispatchDomainEventsAsync();
        
        return await base.SaveChangesAsync(cancellationToken);
    }

    private async Task DispatchDomainEventsAsync()
    {
        var domainEntities = ChangeTracker
            .Entries<Entity<Guid>>()
            .Where(x => x.Entity.DomainEvents?.Any() == true)
            .ToList();

        var domainEvents = domainEntities
            .SelectMany(x => x.Entity.DomainEvents!)
            .ToList();

        domainEntities.ForEach(entity => entity.Entity.ClearDomainEvents());

        foreach (var domainEvent in domainEvents)
        {
            // Here you would publish the domain event using your preferred mechanism
            // For now, we'll just clear them
        }
    }
}