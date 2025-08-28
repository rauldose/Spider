using Microsoft.EntityFrameworkCore;
using Spider.ProjectManagement.Domain.Entities;
using Spider.ProjectManagement.Infrastructure.Configurations;

namespace Spider.ProjectManagement.Infrastructure.Data;

public class ProjectManagementDbContext : DbContext
{
    public DbSet<Project> Projects { get; set; }

    public ProjectManagementDbContext(DbContextOptions<ProjectManagementDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfiguration(new ProjectConfiguration());
        
        // Apply any additional configurations
        modelBuilder.HasDefaultSchema("ProjectManagement");
    }
}