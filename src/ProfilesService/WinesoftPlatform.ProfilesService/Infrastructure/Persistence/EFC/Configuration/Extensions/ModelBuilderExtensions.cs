using Microsoft.EntityFrameworkCore;
using WinesoftPlatform.API.Profiles.Domain.Model.Aggregates;

namespace WinesoftPlatform.API.Profiles.Infrastructure.Persistence.EFC.Configuration.Extensions;

public static class ModelBuilderExtensions
{
    public static void ApplyProfilesConfiguration(this ModelBuilder builder)
    {
        // Profiles Context

        builder.Entity<Profile>().HasKey(p => p.Id);
        builder.Entity<Profile>().Property(p => p.Id).IsRequired().ValueGeneratedOnAdd();
        builder.Entity<Profile>().OwnsOne(p => p.Name,
            n =>
            {
                n.WithOwner().HasForeignKey("Id");
                n.Property(p =>p.BusinessName).HasColumnName("BusinessName");
                n.Property(p =>p.Branch).HasColumnName("Branch");
            });

        builder.Entity<Profile>().OwnsOne(p => p.Address,
            a =>
            {
                a.WithOwner().HasForeignKey("Id");
                a.Property(p => p.Street).HasColumnName("AddressStreet");
                a.Property(p => p.Number).HasColumnName("AddressNumber");
                a.Property(p => p.City).HasColumnName("AddressCity");
                a.Property(p => p.PostalCode).HasColumnName("AddressPostalCode");
                a.Property(p => p.Country).HasColumnName("AddressCountry");
            });
        
        builder.Entity<Profile>().OwnsOne(p => p.Phone,
            ph =>
            {
                ph.WithOwner().HasForeignKey("Id");
                ph.Property(p => p.Number).HasColumnName("PhoneNumber");
            });

        builder.Entity<Profile>().OwnsOne(p => p.LegalId,
            t =>
            {
                t.WithOwner().HasForeignKey("Id");
                t.Property(p => p.Number).HasColumnName("LegalId");
            });
    }
}