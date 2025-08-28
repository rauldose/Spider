using Microsoft.EntityFrameworkCore;
using Spider.DataAcquisition.Domain.Entities;
using Spider.DataAcquisition.Infrastructure.Data.Configurations;

namespace Spider.DataAcquisition.Infrastructure.Data;

/// <summary>
/// Entity Framework DbContext for Data Acquisition
/// </summary>
public class DataAcquisitionDbContext : DbContext
{
    public DataAcquisitionDbContext(DbContextOptions<DataAcquisitionDbContext> options) : base(options) { }

    public DbSet<DataPoint> DataPoints => Set<DataPoint>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfiguration(new DataPointConfiguration());
    }
}