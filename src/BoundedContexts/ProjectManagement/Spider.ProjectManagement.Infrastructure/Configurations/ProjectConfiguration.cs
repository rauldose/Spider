using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Spider.ProjectManagement.Domain.Entities;
using Spider.ProjectManagement.Domain.Enumerations;

namespace Spider.ProjectManagement.Infrastructure.Configurations;

public class ProjectConfiguration : IEntityTypeConfiguration<Project>
{
    public void Configure(EntityTypeBuilder<Project> builder)
    {
        builder.ToTable("Projects");

        builder.HasKey(p => p.Id);

        builder.Property(p => p.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(p => p.Description)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(p => p.Status)
            .HasConversion(
                v => v.Id,
                v => ProjectStatus.From(v))
            .IsRequired();

        builder.Property(p => p.CreatedAt)
            .IsRequired();

        builder.Property(p => p.LastModifiedAt);

        builder.Property(p => p.CreatedBy)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(p => p.LastModifiedBy)
            .HasMaxLength(100);

        // Configure ProjectConfiguration as owned entity
        builder.OwnsOne(p => p.Configuration, config =>
        {
            config.Property(c => c.MaxDevices)
                .HasColumnName("MaxDevices")
                .IsRequired();

            config.Property(c => c.MaxConnections)
                .HasColumnName("MaxConnections")
                .IsRequired();

            config.Property(c => c.DataRetentionPeriod)
                .HasColumnName("DataRetentionPeriod")
                .HasConversion(
                    v => v.TotalDays,
                    v => TimeSpan.FromDays(v))
                .IsRequired();

            config.Property(c => c.EnableRealTimeMonitoring)
                .HasColumnName("EnableRealTimeMonitoring")
                .IsRequired();

            config.Property(c => c.EnableDataArchiving)
                .HasColumnName("EnableDataArchiving")
                .IsRequired();

            config.Property(c => c.EnableAlerting)
                .HasColumnName("EnableAlerting")
                .IsRequired();

            config.Property(c => c.CustomSettings)
                .HasColumnName("CustomSettings")
                .HasConversion(
                    v => System.Text.Json.JsonSerializer.Serialize(v, (System.Text.Json.JsonSerializerOptions?)null),
                    v => System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, string>>(v, (System.Text.Json.JsonSerializerOptions?)null) ?? new Dictionary<string, string>())
                .IsRequired();
        });

        // Configure self-referencing relationship for parent-child projects
        builder.HasOne(p => p.ParentProject)
            .WithMany(p => p.ChildProjects)
            .HasForeignKey(p => p.ParentProjectId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(p => p.Name)
            .IsUnique();

        builder.HasIndex(p => p.Status);
        builder.HasIndex(p => p.CreatedBy);
        builder.HasIndex(p => p.ParentProjectId);
    }
}