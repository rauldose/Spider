using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Spider.ConnectionManagement.Domain.Entities;
using Spider.ConnectionManagement.Domain.Enumerations;
using System.Text.Json;

namespace Spider.ConnectionManagement.Infrastructure.Persistence.Configurations;

public class ConnectionConfiguration : IEntityTypeConfiguration<Connection>
{
    public void Configure(EntityTypeBuilder<Connection> builder)
    {
        builder.ToTable("Connections");

        builder.HasKey(c => c.Id);

        builder.Property(c => c.Id)
            .ValueGeneratedNever();

        builder.Property(c => c.DeviceId)
            .IsRequired();

        builder.Property(c => c.Name)
            .IsRequired()
            .HasMaxLength(100);

        // Protocol configuration
        builder.Property(c => c.Protocol)
            .HasConversion(
                p => p.Id,
                id => ProtocolType.GetAll<ProtocolType>().First(p => p.Id == id))
            .IsRequired();

        // Connection Parameters as owned entity
        builder.OwnsOne(c => c.Parameters, param =>
        {
            param.Property(p => p.Host)
                .IsRequired()
                .HasMaxLength(255)
                .HasColumnName("Host");

            param.Property(p => p.Port)
                .IsRequired()
                .HasColumnName("Port");

            param.Property(p => p.TimeoutMs)
                .IsRequired()
                .HasColumnName("TimeoutMs");

            param.Property(p => p.RetryAttempts)
                .IsRequired()
                .HasColumnName("RetryAttempts");

            param.Property(p => p.ExtendedProperties)
                .HasConversion(
                    props => JsonSerializer.Serialize(props, (JsonSerializerOptions?)null),
                    json => JsonSerializer.Deserialize<Dictionary<string, object>>(json, (JsonSerializerOptions?)null) ?? new Dictionary<string, object>())
                .HasColumnName("ExtendedProperties")
                .HasColumnType("nvarchar(max)");
        });

        // Connection Status configuration
        builder.Property(c => c.Status)
            .HasConversion(
                s => s.Id,
                id => ConnectionStatus.GetAll<ConnectionStatus>().First(s => s.Id == id))
            .IsRequired();

        // Connection Health as owned entity
        builder.OwnsOne(c => c.Health, health =>
        {
            health.Property(h => h.IsHealthy)
                .IsRequired()
                .HasColumnName("IsHealthy");

            health.Property(h => h.ResponseTimeMs)
                .IsRequired()
                .HasColumnName("ResponseTimeMs");

            health.Property(h => h.ConsecutiveFailures)
                .IsRequired()
                .HasColumnName("ConsecutiveFailures");

            health.Property(h => h.LastHealthCheck)
                .IsRequired()
                .HasColumnName("LastHealthCheck");

            health.Property(h => h.LastError)
                .HasMaxLength(500)
                .HasColumnName("LastHealthError");
        });

        builder.Property(c => c.CreatedAt)
            .IsRequired();

        builder.Property(c => c.LastConnectedAt);

        builder.Property(c => c.LastDisconnectedAt);

        builder.Property(c => c.LastErrorMessage)
            .HasMaxLength(500);

        // Indexes
        builder.HasIndex(c => c.DeviceId);
        builder.HasIndex(c => new { c.DeviceId, c.Name }).IsUnique();
        builder.HasIndex(c => c.Status);
        builder.HasIndex(c => c.CreatedAt);

        // Domain events are handled by the SharedKernel
        builder.Ignore(c => c.DomainEvents);
    }
}