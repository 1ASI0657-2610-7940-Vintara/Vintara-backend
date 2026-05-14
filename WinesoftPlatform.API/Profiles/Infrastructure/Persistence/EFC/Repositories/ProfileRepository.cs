using Microsoft.EntityFrameworkCore;
using WinesoftPlatform.API.Profiles.Domain.Model.Aggregates;
using WinesoftPlatform.API.Profiles.Domain.Model.ValueObjects;
using WinesoftPlatform.API.Profiles.Domain.Repositories;
using WinesoftPlatform.API.Shared.Infrastructure.Persistence.EFC.Configuration;
using WinesoftPlatform.API.Shared.Infrastructure.Persistence.EFC.Repositories;

namespace WinesoftPlatform.API.Profiles.Infrastructure.Persistence.EFC.Repositories;

public class ProfileRepository(AppDbContext context) 
    : BaseRepository<Profile>(context), IProfileRepository
{
    public async Task<Profile?> FindProfileByTaxIdentityAsync(TaxIdentity legalId)
    {
        return await Context.Set<Profile>().FirstOrDefaultAsync(p => p.LegalId == legalId);
    }
}