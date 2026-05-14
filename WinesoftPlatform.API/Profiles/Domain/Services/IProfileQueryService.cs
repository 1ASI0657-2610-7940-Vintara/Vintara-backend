using WinesoftPlatform.API.Profiles.Domain.Model.Aggregates;
using WinesoftPlatform.API.Profiles.Domain.Model.Queries;

namespace WinesoftPlatform.API.Profiles.Domain.Services;

/// <summary>
///     Profile query service
/// </summary>
public interface IProfileQueryService
{
    /// <summary>
    ///     Handle get all profiles
    /// </summary>
    /// <param name="query">
    ///     The <see cref="GetAllProfilesQuery" /> query
    /// </param>
    /// <returns>
    ///     A list of <see cref="Profile" /> objects
    /// </returns>
    Task<IEnumerable<Profile>> Handle(GetAllProfilesQuery query);
    
    Task<Profile?> Handle(GetProfileByTaxIdentityQuery query);
}