using WinesoftPlatform.API.Profiles.Domain.Model.Aggregates;
using WinesoftPlatform.API.Profiles.Domain.Model.Queries;
using WinesoftPlatform.API.Profiles.Domain.Repositories;
using WinesoftPlatform.API.Profiles.Domain.Services;

namespace WinesoftPlatform.API.Profiles.Application.Internal.QueryServices;

/// <summary>
///     Profile query service
/// </summary>
/// <param name="profileRepository">
///     Profile repository
/// </param>
public class ProfileQueryService(IProfileRepository profileRepository) : IProfileQueryService
{
    /// <inheritdoc />
    public async Task<IEnumerable<Profile>> Handle(GetAllProfilesQuery query)
    {
        return await profileRepository.ListAsync();
    }

    public async Task<Profile?> Handle(GetProfileByTaxIdentityQuery query)
    {
        return await profileRepository.FindProfileByTaxIdentityAsync(query.LegalId);
    }
}