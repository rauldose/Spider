using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Spider.DataAcquisition.Domain.Entities;
using Spider.DataAcquisition.Domain.Enumerations;
using Spider.DataAcquisition.Domain.ValueObjects;

namespace Spider.DataAcquisition.Infrastructure.Data.Configurations;

/// <summary>
/// Entity Framework configuration for DataPoint
/// </summary>
public class DataPointConfiguration : IEntityTypeConfiguration<DataPoint>
{
    public void Configure(EntityTypeBuilder<DataPoint> builder)
    {
        builder.ToTable("DataPoints");

        builder.HasKey(dp => dp.Id);

        builder.Property(dp => dp.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(dp => dp.Description)
            .HasMaxLength(500);

        builder.Property(dp => dp.DeviceId)
            .IsRequired();

        builder.Property(dp => dp.IsEnabled)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(dp => dp.ScanInterval)
            .IsRequired()
            .HasDefaultValue(1000);

        builder.Property(dp => dp.LastScanTime);

        // Configure DataAddress value object
        builder.OwnsOne(dp => dp.Address, addressBuilder =>
        {
            addressBuilder.Property(a => a.Address)
                .IsRequired()
                .HasMaxLength(200)
                .HasColumnName("Address");

            addressBuilder.Property(a => a.Group)
                .HasMaxLength(50)
                .HasColumnName("AddressGroup");

            addressBuilder.Property(a => a.Register)
                .HasColumnName("AddressRegister");
        });

        // Configure DataType enumeration
        builder.Property(dp => dp.DataType)
            .HasConversion(
                dt => dt.Id,
                id => DataType.GetAll().First(dt => dt.Id == id))
            .IsRequired()
            .HasColumnName("DataTypeId");

        // Configure LastValue value object
        builder.OwnsOne(dp => dp.LastValue, valueBuilder =>
        {
            valueBuilder.Property(v => v.Value)
                .HasColumnName("LastValue")
                .HasConversion<string>();

            valueBuilder.Property(v => v.Timestamp)
                .HasColumnName("LastValueTimestamp");

            valueBuilder.Property(v => v.Quality)
                .HasConversion(
                    q => q.Id,
                    id => DataQuality.GetAll().First(q => q.Id == id))
                .HasColumnName("LastValueQuality");

            valueBuilder.Property(v => v.DataType)
                .HasConversion(
                    dt => dt.Id,
                    id => DataType.GetAll().First(dt => dt.Id == id))
                .HasColumnName("LastValueDataType");
        });

        // Index for efficient queries
        builder.HasIndex(dp => dp.DeviceId);
        builder.HasIndex(dp => dp.IsEnabled);
    }
}