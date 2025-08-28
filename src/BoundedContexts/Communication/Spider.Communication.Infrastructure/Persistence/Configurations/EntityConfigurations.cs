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

            config.Property(c => c.OperationTimeout)
                .HasConversion(
                    v => v.TotalMilliseconds,
                    v => TimeSpan.FromMilliseconds(v));

            config.Property(c => c.HealthCheckInterval)
                .HasConversion(
                    v => v.TotalMilliseconds,
                    v => TimeSpan.FromMilliseconds(v));

            config.Property(c => c.MaxChannels);

            config.Property(c => c.AutoReconnect);

            config.Property(c => c.MaxRetryAttempts);
        });

        // Configure LinkHealth as owned type
        builder.OwnsOne(l => l.Health, health =>
        {
            health.Property(h => h.IsHealthy);

            health.Property(h => h.Status)
                .HasMaxLength(50);

            health.Property(h => h.ErrorMessage)
                .HasMaxLength(1000);

            health.Property(h => h.LastChecked);
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

        // Configure indexes (using the owned type properties)
        builder.HasIndex("Metadata_Name")
            .IsUnique();

        builder.HasIndex(l => l.Status);

        builder.HasIndex("Metadata_ProtocolType");
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

        builder.Property(c => c.Type)
            .HasConversion<string>()
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(c => c.Status)
            .HasConversion<string>()
            .HasMaxLength(50)
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

        builder.HasIndex(c => c.Type);

        builder.HasIndex(c => c.Status);
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

        builder.Property(dp => dp.Address)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(dp => dp.DataType)
            .HasConversion<string>()
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(dp => dp.Length);

        builder.Property(dp => dp.IsWritable)
            .IsRequired();

        builder.Property(dp => dp.CurrentValue)
            .HasConversion(
                v => v != null ? JsonSerializer.Serialize(v, (JsonSerializerOptions?)null) : null,
                v => v != null ? JsonSerializer.Deserialize<object>(v, (JsonSerializerOptions?)null) : null)
            .HasColumnType("nvarchar(max)");

        builder.Property(dp => dp.DataQuality)
            .HasMaxLength(50);

        builder.Property(dp => dp.LastUpdated);

        builder.Property(dp => dp.CreatedAt)
            .IsRequired();

        // Configure indexes
        builder.HasIndex(dp => dp.ChannelId);

        builder.HasIndex(dp => new { dp.ChannelId, dp.Name })
            .IsUnique();

        builder.HasIndex(dp => dp.Address);

        builder.HasIndex(dp => dp.DataType);
    }
}