using EntityFrameworkCore.CreatedUpdatedDate.Extensions;
using Microsoft.EntityFrameworkCore;
using WinesoftPlatform.API.Inventory.Domain.Model.Aggregates;
using WinesoftPlatform.API.Shared.Infrastructure.Persistence.EFC.Configuration.Extensions;

namespace WinesoftPlatform.InventoryService.Infrastructure.Persistence.EFC.Configuration;

public class InventoryDbContext(DbContextOptions options) : DbContext(options)
{
    public DbSet<Supply> Supplies { get; set; }

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
        });
        
        builder.UseSnakeCaseNamingConvention();
    }
}
