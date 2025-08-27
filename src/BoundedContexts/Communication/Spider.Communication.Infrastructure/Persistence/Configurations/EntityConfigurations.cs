using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Spider.Communication.Domain.Entities;
using Spider.Communication.Domain.ValueObjects;
using System.Text.Json;

namespace Spider.Communication.Infrastructure.Persistence.Configurations;

/// <summary>
/// EF Core configuration for Link entity
/// </summary>
public class LinkConfiguration : IEntityTypeConfiguration<Link>
{
    public void Configure(EntityTypeBuilder<Link> builder)
    {
        builder.ToTable("Links");

        builder.HasKey(l => l.Id);

        builder.Property(l => l.Id)
            .ValueGeneratedNever();

        // Configure LinkMetadata as owned type
        builder.OwnsOne(l => l.Metadata, metadata =>
        {
            metadata.Property(m => m.Name)
                .HasMaxLength(100)
                .IsRequired();

            metadata.Property(m => m.Description)
                .HasMaxLength(500);

            metadata.Property(m => m.ProtocolType)
                .HasMaxLength(50)
                .IsRequired();
        });

        // Configure LinkConfiguration as owned type
        builder.OwnsOne(l => l.Configuration, config =>
        {
            config.Property(c => c.ConnectionString)
                .HasMaxLength(1000)
                .IsRequired();

            config.Property(c => c.Parameters)
                .HasConversion(
                    v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                    v => JsonSerializer.Deserialize<Dictionary<string, object>>(v, (JsonSerializerOptions?)null) ?? new Dictionary<string, object>())
                .HasColumnType("nvarchar(max)");

            config.Property(c => c.ConnectionTimeout)
                .HasConversion(
                    v => v.TotalMilliseconds,
                    v => TimeSpan.FromMilliseconds(v));

            config.Property(c => c.ReadTimeout)
                .HasConversion(
                    v => v.TotalMilliseconds,
                    v => TimeSpan.FromMilliseconds(v));

            config.Property(c => c.MaxRetries);

            config.Property(c => c.EnableHeartbeat);

            config.Property(c => c.HeartbeatInterval)
                .HasConversion(
                    v => v.TotalMilliseconds,
                    v => TimeSpan.FromMilliseconds(v));
        });

        // Configure LinkHealth as owned type
        builder.OwnsOne(l => l.Health, health =>
        {
            health.Property(h => h.IsHealthy);

            health.Property(h => h.SuccessRate)
                .HasPrecision(5, 2);

            health.Property(h => h.AverageResponseTime)
                .HasConversion(
                    v => v.TotalMilliseconds,
                    v => TimeSpan.FromMilliseconds(v));

            health.Property(h => h.ErrorCount);

            health.Property(h => h.LastError);

            health.Property(h => h.LastErrorMessage)
                .HasMaxLength(1000);
        });

        // Configure LinkStatus as enum
        builder.Property(l => l.Status)
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.Property(l => l.CreatedAt)
            .IsRequired();

        builder.Property(l => l.LastActivity)
            .IsRequired();

        // Configure relationships
        builder.HasMany(l => l.Channels)
            .WithOne()
            .HasForeignKey(c => c.LinkId)
            .OnDelete(DeleteBehavior.Cascade);

        // Ignore Driver property (not persisted)
        builder.Ignore(l => l.Driver);

        // Configure indexes
        builder.HasIndex(l => l.Metadata.Name)
            .IsUnique();

        builder.HasIndex(l => l.Status);

        builder.HasIndex(l => l.Metadata.ProtocolType);
    }
}

/// <summary>
/// EF Core configuration for Channel entity
/// </summary>
public class ChannelConfiguration : IEntityTypeConfiguration<Channel>
{
    public void Configure(EntityTypeBuilder<Channel> builder)
    {
        builder.ToTable("Channels");

        builder.HasKey(c => c.Id);

        builder.Property(c => c.Id)
            .ValueGeneratedNever();

        builder.Property(c => c.LinkId)
            .IsRequired();

        builder.Property(c => c.Name)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(c => c.Description)
            .HasMaxLength(500);

        builder.Property(c => c.ChannelType)
            .HasConversion<string>()
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(c => c.IsEnabled)
            .IsRequired();

        builder.Property(c => c.CreatedAt)
            .IsRequired();

        // Configure relationships
        builder.HasMany(c => c.DataPoints)
            .WithOne()
            .HasForeignKey(dp => dp.ChannelId)
            .OnDelete(DeleteBehavior.Cascade);

        // Configure indexes
        builder.HasIndex(c => c.LinkId);

        builder.HasIndex(c => new { c.LinkId, c.Name })
            .IsUnique();

        builder.HasIndex(c => c.ChannelType);

        builder.HasIndex(c => c.IsEnabled);
    }
}

/// <summary>
/// EF Core configuration for DataPoint entity
/// </summary>
public class DataPointConfiguration : IEntityTypeConfiguration<DataPoint>
{
    public void Configure(EntityTypeBuilder<DataPoint> builder)
    {
        builder.ToTable("DataPoints");

        builder.HasKey(dp => dp.Id);

        builder.Property(dp => dp.Id)
            .ValueGeneratedNever();

        builder.Property(dp => dp.ChannelId)
            .IsRequired();

        builder.Property(dp => dp.Name)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(dp => dp.Description)
            .HasMaxLength(500);

        // Configure DataAddress as owned type
        builder.OwnsOne(dp => dp.Address, address =>
        {
            address.Property(a => a.Value)
                .HasMaxLength(200)
                .IsRequired()
                .HasColumnName("Address");
        });

        builder.Property(dp => dp.DataType)
            .HasConversion<string>()
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(dp => dp.AccessMode)
            .HasConversion<string>()
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(dp => dp.IsEnabled)
            .IsRequired();

        builder.Property(dp => dp.CreatedAt)
            .IsRequired();

        // Configure CurrentValue as owned type
        builder.OwnsOne(dp => dp.CurrentValue, value =>
        {
            value.Property(v => v.Value)
                .HasConversion(
                    v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                    v => JsonSerializer.Deserialize<object>(v, (JsonSerializerOptions?)null))
                .HasColumnType("nvarchar(max)")
                .HasColumnName("CurrentValue");

            value.Property(v => v.Quality)
                .HasConversion<string>()
                .HasMaxLength(50)
                .HasColumnName("Quality");

            value.Property(v => v.Timestamp)
                .HasColumnName("ValueTimestamp");

            value.Property(v => v.Source)
                .HasMaxLength(100)
                .HasColumnName("ValueSource");
        });

        // Configure indexes
        builder.HasIndex(dp => dp.ChannelId);

        builder.HasIndex(dp => new { dp.ChannelId, dp.Name })
            .IsUnique();

        builder.HasIndex(dp => dp.Address.Value);

        builder.HasIndex(dp => dp.DataType);

        builder.HasIndex(dp => dp.AccessMode);

        builder.HasIndex(dp => dp.IsEnabled);
    }
}