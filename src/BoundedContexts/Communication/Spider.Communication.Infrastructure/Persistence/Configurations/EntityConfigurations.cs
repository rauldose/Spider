using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Spider.Communication.Domain.Entities;
using Spider.Communication.Domain.ValueObjects;
using Spider.Core.SharedKernel.Base;
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

        // Configure LinkMetadata as JSON
        builder.Property(l => l.Metadata)
            .HasConversion(
                v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                v => JsonSerializer.Deserialize<LinkMetadata>(v, (JsonSerializerOptions?)null)!)
            .HasColumnType("nvarchar(max)");

        // Configure LinkConfiguration as JSON
        builder.Property(l => l.Configuration)
            .HasConversion(
                v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                v => JsonSerializer.Deserialize<Spider.Communication.Domain.ValueObjects.LinkConfiguration>(v, (JsonSerializerOptions?)null)!)
            .HasColumnType("nvarchar(max)");

        // Configure LinkHealth as JSON
        builder.Property(l => l.Health)
            .HasConversion(
                v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                v => JsonSerializer.Deserialize<LinkHealth>(v, (JsonSerializerOptions?)null)!)
            .HasColumnType("nvarchar(max)");

        // Configure LinkStatus as enum
        builder.Property(l => l.Status)
            .HasConversion(
                v => v.Name,
                v => Enumeration.FromDisplayName<LinkStatus>(v))
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
        builder.HasIndex(l => l.Status);
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
            .HasConversion(
                v => v.Name,
                v => Enumeration.FromDisplayName<ChannelType>(v))
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(c => c.Status)
            .HasConversion(
                v => v.Name,
                v => Enumeration.FromDisplayName<ChannelStatus>(v))
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(c => c.CreatedAt)
            .IsRequired();

        builder.Property(c => c.LastActivity)
            .IsRequired();

        // Configure ChannelConfiguration as JSON
        builder.Property(c => c.Configuration)
            .HasConversion(
                v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                v => JsonSerializer.Deserialize<Spider.Communication.Domain.ValueObjects.ChannelConfiguration>(v, (JsonSerializerOptions?)null)!)
            .HasColumnType("nvarchar(max)");

        // Configure ChannelHealth as JSON
        builder.Property(c => c.Health)
            .HasConversion(
                v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                v => JsonSerializer.Deserialize<ChannelHealth>(v, (JsonSerializerOptions?)null)!)
            .HasColumnType("nvarchar(max)");

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
            .HasConversion(
                v => v.Name,
                v => Enumeration.FromDisplayName<DataPointType>(v))
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

        // Configure DataPointConfiguration as JSON
        builder.Property(dp => dp.Configuration)
            .HasConversion(
                v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                v => JsonSerializer.Deserialize<Spider.Communication.Domain.ValueObjects.DataPointConfiguration>(v, (JsonSerializerOptions?)null)!)
            .HasColumnType("nvarchar(max)");

        // Configure indexes
        builder.HasIndex(dp => dp.ChannelId);

        builder.HasIndex(dp => new { dp.ChannelId, dp.Name })
            .IsUnique();

        builder.HasIndex(dp => dp.Address);

        builder.HasIndex(dp => dp.DataType);
    }
}