using EntityFrameworkCore.CreatedUpdatedDate.Extensions;
using Microsoft.EntityFrameworkCore;
using WinesoftPlatform.API.Inventory.Domain.Model.Aggregates;
using WinesoftPlatform.API.Shared.Infrastructure.Persistence.EFC.Configuration.Extensions;

namespace WinesoftPlatform.InventoryService.Infrastructure.Persistence.EFC.Configuration;

public class InventoryDbContext(DbContextOptions<InventoryDbContext> options) : DbContext(options)
{
    public DbSet<Supply> Supplies { get; set; }
    public DbSet<SensorAlert> SensorAlerts { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder builder)
    {
        builder.AddCreatedUpdatedInterceptor();
        base.OnConfiguring(builder);
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        
        builder.Entity<Supply>(entity =>
        {
            entity.ToTable("supplies");
            entity.HasKey(s => s.Id);
            entity.Property(s => s.Id).HasColumnName("id").ValueGeneratedOnAdd();
            entity.Property(s => s.SupplyName).HasColumnName("supply_name").HasMaxLength(255).IsRequired();
            entity.Property(s => s.Quantity).HasColumnName("quantity").IsRequired();
            entity.Property(s => s.Unit).HasColumnName("unit").HasMaxLength(50).IsRequired();
            entity.Property(s => s.Supplier).HasColumnName("supplier").HasMaxLength(255).IsRequired();
            entity.Property(s => s.Price).HasColumnName("price").HasColumnType("decimal(10,2)").IsRequired();
            entity.Property(s => s.Date).HasColumnName("date").IsRequired();
            entity.Property(s => s.OwnerId).HasColumnName("owner_id").IsRequired();
        });

        builder.Entity<SensorAlert>(entity =>
        {
            entity.ToTable("sensor_alerts");
            entity.HasKey(a => a.Id);
            entity.Property(a => a.Id).HasColumnName("id").ValueGeneratedOnAdd();
            entity.Property(a => a.DeviceId).HasColumnName("device_id").HasMaxLength(100).IsRequired();
            entity.Property(a => a.SensorType).HasColumnName("sensor_type").HasMaxLength(50).IsRequired();
            entity.Property(a => a.Value).HasColumnName("value").IsRequired();
            entity.Property(a => a.Unit).HasColumnName("unit").HasMaxLength(20).IsRequired();
            entity.Property(a => a.Timestamp).HasColumnName("timestamp").IsRequired();
            entity.Property(a => a.Status).HasColumnName("status").HasMaxLength(20).IsRequired();
            entity.Property(a => a.IsAnomaly).HasColumnName("is_anomaly").IsRequired();
            entity.Property(a => a.Acknowledged).HasColumnName("acknowledged").HasDefaultValue(false);
            entity.Property(a => a.AcknowledgedAt).HasColumnName("acknowledged_at");
            entity.Property(a => a.OwnerId).HasColumnName("owner_id").IsRequired();

            entity.HasIndex(a => a.Status);
            entity.HasIndex(a => a.SensorType);
            entity.HasIndex(a => a.Timestamp);
        });
        
        builder.UseSnakeCaseNamingConvention();
    }
}
