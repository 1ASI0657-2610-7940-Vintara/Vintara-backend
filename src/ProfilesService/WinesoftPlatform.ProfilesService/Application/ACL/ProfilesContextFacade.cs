using WinesoftPlatform.API.Profiles.Domain.Model.Aggregates;
using WinesoftPlatform.API.Profiles.Domain.Model.Commands;
using WinesoftPlatform.API.Profiles.Domain.Model.Queries;
using WinesoftPlatform.API.Profiles.Domain.Model.ValueObjects;
using WinesoftPlatform.API.Profiles.Domain.Services;
using WinesoftPlatform.API.Profiles.Interfaces.ACL;

namespace WinesoftPlatform.API.Profiles.Application.ACL;

/// <summary>
///     Facade for the profiles context
/// </summary>
/// <param name="profileCommandService">
///     The profile command service
/// </param>
/// <param name="profileQueryService">
///     The profile query service
/// </param>
public class ProfilesContextFacade(
    IProfileCommandService profileCommandService,
    IProfileQueryService profileQueryService
) : IProfilesContextFacade
{
    public async Task<int> CreateProfile(string businessName, string branch, string street, string number,
        string city,
        string postalCode, string country, string phone, string legalId)
    {
        var createProfileCommand =
            new CreateProfileCommand(businessName, branch, street, number, city, postalCode, country, phone, legalId);
        var profile = await profileCommandService.Handle(createProfileCommand);
        return profile?.Id ?? 0;
    }

    public async Task<int> FetchProfileByTaxIdentity(string legalId)
    {
        var getProfileByTaxIdentityQuery = new GetProfileByTaxIdentityQuery(new TaxIdentity(legalId));
        var profile = await profileQueryService.Handle(getProfileByTaxIdentityQuery);
        return profile?.Id ?? 0;
    }
}