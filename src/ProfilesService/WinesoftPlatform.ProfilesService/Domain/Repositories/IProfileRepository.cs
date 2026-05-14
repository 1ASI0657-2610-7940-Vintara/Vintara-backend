using WinesoftPlatform.API.Profiles.Domain.Model.Aggregates;
using WinesoftPlatform.API.Profiles.Domain.Model.ValueObjects;
using WinesoftPlatform.API.Shared.Domain.Repositories;

namespace WinesoftPlatform.API.Profiles.Domain.Repositories;

public interface IProfileRepository : IBaseRepository<Profile>
{
    Task<Profile?> FindProfileByTaxIdentityAsync(TaxIdentity legalId);
}