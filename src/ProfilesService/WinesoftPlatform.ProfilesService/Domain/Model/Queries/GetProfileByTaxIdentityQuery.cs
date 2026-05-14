using WinesoftPlatform.API.Profiles.Domain.Model.ValueObjects;

namespace WinesoftPlatform.API.Profiles.Domain.Model.Queries;

/// <summary>
/// Get Profile By Tax Identity
/// </summary>
/// <param name="LegalId">
/// The <see cref="TaxIdentity"/> of the profile to retrieve
/// </param>
public record GetProfileByTaxIdentityQuery(TaxIdentity LegalId);