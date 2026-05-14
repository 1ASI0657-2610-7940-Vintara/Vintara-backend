using EntityFrameworkCore.CreatedUpdatedDate.Extensions;
using Microsoft.EntityFrameworkCore;
using WinesoftPlatform.API.Profiles.Domain.Model.Aggregates;
using WinesoftPlatform.API.Shared.Infrastructure.Persistence.EFC.Configuration.Extensions;

namespace WinesoftPlatform.ProfilesService.Infrastructure.Persistence.EFC.Configuration;

public class ProfilesDbContext(DbContextOptions options) : DbContext(options)
{
    public DbSet<Profile> Profiles { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder builder)
    {
        builder.AddCreatedUpdatedInterceptor();
        base.OnConfiguring(builder);
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        
        builder.Entity<Profile>(entity =>
        {
            entity.ToTable("profiles");
            entity.HasKey(p => p.Id);
            entity.Property(p => p.Id).HasColumnName("id").ValueGeneratedOnAdd();
            
            entity.OwnsOne(p => p.Name, n =>
            {
                n.WithOwner().HasForeignKey("Id");
                n.Property(name => name.BusinessName).HasColumnName("business_name").HasMaxLength(255).IsRequired();
                n.Property(name => name.Branch).HasColumnName("branch").HasMaxLength(255).IsRequired();
            });
            
            entity.OwnsOne(p => p.Address, a =>
            {
                a.WithOwner().HasForeignKey("Id");
                a.Property(address => address.Street).HasColumnName("street").HasMaxLength(255).IsRequired();
                a.Property(address => address.Number).HasColumnName("number").HasMaxLength(50).IsRequired();
                a.Property(address => address.City).HasColumnName("city").HasMaxLength(255).IsRequired();
                a.Property(address => address.PostalCode).HasColumnName("postal_code").HasMaxLength(50).IsRequired();
                a.Property(address => address.Country).HasColumnName("country").HasMaxLength(255).IsRequired();
            });
            
            entity.OwnsOne(p => p.Phone, ph =>
            {
                ph.WithOwner().HasForeignKey("Id");
                ph.Property(phone => phone.Number).HasColumnName("phone").HasMaxLength(50).IsRequired();
            });
            
            entity.OwnsOne(p => p.LegalId, l =>
            {
                l.WithOwner().HasForeignKey("Id");
                l.Property(legalId => legalId.Number).HasColumnName("legal_id").HasMaxLength(50).IsRequired();
            });
            
            entity.Property(p => p.CreatedDate).HasColumnName("created_at").IsRequired(false);
            entity.Property(p => p.UpdatedDate).HasColumnName("updated_at").IsRequired(false);
        });
        
        builder.UseSnakeCaseNamingConvention();
    }
}
