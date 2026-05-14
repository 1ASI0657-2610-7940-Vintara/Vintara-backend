using EntityFrameworkCore.CreatedUpdatedDate.Extensions;
using Microsoft.EntityFrameworkCore;
using WinesoftPlatform.API.Purchase.Domain.Model.Aggregates;
using WinesoftPlatform.API.Shared.Infrastructure.Persistence.EFC.Configuration.Extensions;

namespace WinesoftPlatform.PurchaseService.Infrastructure.Persistence.EFC.Configuration;

public class PurchaseDbContext(DbContextOptions options) : DbContext(options)
{
    public DbSet<Order> Orders { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder builder)
    {
        builder.AddCreatedUpdatedInterceptor();
        base.OnConfiguring(builder);
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        
        builder.Entity<Order>(entity =>
        {
            entity.ToTable("orders");
            entity.HasKey(o => o.Id);
            entity.Property(o => o.Id).HasColumnName("id").ValueGeneratedOnAdd();
            entity.Property(o => o.ProductId).HasColumnName("product_id").IsRequired();
            entity.Property(o => o.SupplyName).HasColumnName("supply_name").HasMaxLength(255).IsRequired();
            entity.Property(o => o.Supplier).HasColumnName("supplier").HasMaxLength(255).IsRequired();
            entity.Property(o => o.Quantity).HasColumnName("quantity").IsRequired();
            entity.Property(o => o.Status).HasColumnName("status").HasMaxLength(50).IsRequired();
            entity.Property(o => o.CreatedDate).HasColumnName("created_at").IsRequired(false);
            entity.Property(o => o.UpdatedDate).HasColumnName("updated_at").IsRequired(false);
        });
        
        builder.UseSnakeCaseNamingConvention();
    }
}
