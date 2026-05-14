using EntityFrameworkCore.CreatedUpdatedDate.Extensions;
using Microsoft.EntityFrameworkCore;
using WinesoftPlatform.API.Inventory.Domain.Model.Aggregates;
using WinesoftPlatform.API.Purchase.Domain.Model.Aggregates; // Import actualizado
using WinesoftPlatform.API.Profiles.Infrastructure.Persistence.EFC.Configuration.Extensions;
using WinesoftPlatform.API.Shared.Domain.Model;
using WinesoftPlatform.API.Shared.Infrastructure.Persistence.EFC.Configuration.Extensions;

namespace WinesoftPlatform.API.Shared.Infrastructure.Persistence.EFC.Configuration;

public class AppDbContext(DbContextOptions options) : DbContext(options)
{
    public DbSet<Order> Orders { get; set; }
    public DbSet<Supply> Supplies { get; set; }
    public DbSet<User> Users { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder builder)
    {
        builder.AddCreatedUpdatedInterceptor();
        base.OnConfiguring(builder);
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        
        // Purchase Context
        builder.Entity<Order>().ToTable("orders");
        builder.Entity<Order>().HasKey(o => o.Id);
        builder.Entity<Order>().Property(o => o.Id).IsRequired().ValueGeneratedOnAdd();
        builder.Entity<Order>().Property(o => o.ProductId).IsRequired();
        builder.Entity<Order>().Property(o => o.Supplier).IsRequired().HasMaxLength(100);
        builder.Entity<Order>().Property(o => o.Quantity).IsRequired();
        builder.Entity<Order>().Property(o => o.Status).IsRequired().HasMaxLength(30);
        builder.Entity<Order>().Property(o => o.CreatedDate);
        builder.Entity<Order>().Property(o => o.UpdatedDate);
        builder.Entity<User>(entity =>
        {
            entity.ToTable("users");
            entity.HasKey(u => u.Id);
            entity.Property(u => u.Id).HasColumnName("id").ValueGeneratedOnAdd();
            entity.Property(u => u.Email).HasColumnName("email").HasMaxLength(255).IsRequired();
            entity.Property(u => u.PasswordHash).HasColumnName("password_hash").HasMaxLength(500).IsRequired();
            entity.Property(u => u.CreatedAt).HasColumnName("created_at").IsRequired();
            entity.Property(u => u.UpdatedAt).HasColumnName("updated_at").IsRequired();
            
            // Unique constraint for email
            entity.HasIndex(u => u.Email).IsUnique();
        });
        // Inventory Context
        builder.Entity<Supply>().ToTable("supplies");
        builder.Entity<Supply>().HasKey(s => s.Id);
        builder.Entity<Supply>().Property(s => s.SupplyName).IsRequired();
        builder.Entity<Supply>().Property(s => s.Quantity).IsRequired();
        builder.Entity<Supply>().Property(s => s.Unit).IsRequired();
        builder.Entity<Supply>().Property(s => s.Supplier).IsRequired();
        builder.Entity<Supply>().Property(s => s.Price).IsRequired();
        builder.Entity<Supply>().Property(s => s.Date).IsRequired();
        
        builder.ApplyProfilesConfiguration();
        
        // Apply naming convention to use snake_case for database objects
        builder.UseSnakeCaseNamingConvention();
    }
}