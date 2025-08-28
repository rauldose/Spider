using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Spider.DeviceManagement.Domain.Entities;
using Spider.DeviceManagement.Domain.Enums;
using Spider.DeviceManagement.Domain.ValueObjects;

namespace Spider.DeviceManagement.Infrastructure.Persistence;

/// <summary>
/// Entity Framework context for Device Management bounded context
/// </summary>
public class DeviceManagementDbContext : DbContext
{
    public DbSet<Device> Devices { get; set; }

    public DeviceManagementDbContext(DbContextOptions<DeviceManagementDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Device configuration
        modelBuilder.Entity<Device>(entity =>
        {
            entity.HasKey(e => e.Id);
            
            entity.Property(e => e.Name)
                .IsRequired()
                .HasMaxLength(100);

            entity.Property(e => e.Description)
                .HasMaxLength(500);

            entity.Property(e => e.LastError)
                .HasMaxLength(1000);

            // Protocol enumeration mapping
            entity.Property(e => e.Protocol)
                .HasConversion(
                    v => v.Id,
                    v => ProtocolType.FromValue<ProtocolType>(v))
                .IsRequired();

            // Status enumeration mapping
            entity.Property(e => e.Status)
                .HasConversion(
                    v => v.Id,
                    v => DeviceStatus.FromValue<DeviceStatus>(v))
                .IsRequired();

            // ConnectionParameters value object mapping
            entity.OwnsOne(e => e.ConnectionParameters, cp =>
            {
                cp.Property(p => p.Host)
                    .IsRequired()
                    .HasMaxLength(255)
                    .HasColumnName("ConnectionHost");

                cp.Property(p => p.Port)
                    .IsRequired()
                    .HasColumnName("ConnectionPort");

                cp.Property(p => p.Timeout)
                    .HasColumnName("ConnectionTimeout");

                cp.Property(p => p.RetryCount)
                    .HasColumnName("ConnectionRetryCount");

                // Store additional parameters as JSON
                cp.Property(p => p.AdditionalParameters)
                    .HasConversion(
                        v => System.Text.Json.JsonSerializer.Serialize(v, (System.Text.Json.JsonSerializerOptions?)null),
                        v => System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, string>>(v, (System.Text.Json.JsonSerializerOptions?)null) ?? new Dictionary<string, string>())
                    .HasColumnName("ConnectionAdditionalParameters");
            });

            // Indexes
            entity.HasIndex(e => e.ProjectId);
            entity.HasIndex(e => e.Name);
            entity.HasIndex(e => e.Status);

            // Ignore domain events for persistence
            entity.Ignore(e => e.DomainEvents);
        });
    }
}