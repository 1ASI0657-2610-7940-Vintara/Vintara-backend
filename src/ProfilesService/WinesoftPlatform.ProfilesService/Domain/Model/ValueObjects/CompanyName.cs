namespace WinesoftPlatform.API.Profiles.Domain.Model.ValueObjects;

/// <summary>
///     Value object that represents the company's operational identity.
/// </summary>
/// <param name="BusinessName">
///     The company's main name (e.g. "WineSoft", "Vinos S.A.")
/// </param>
/// <param name="Branch">
///     The specific branch, office, or division (e.g. "Head Office", "North Warehouse", "Store 1")
/// </param>
public record CompanyName(string BusinessName, string Branch)
{
    // Parameterless constructor for EF Core
    public CompanyName() : this(string.Empty, string.Empty)
    {
    }

    public CompanyName(string businessName) : this(businessName, string.Empty)
    {
    }

    // Returns the full display name for lists or order headers
    // E.g.: "Vinos S.A. - Head Office"
    public string FullName => string.IsNullOrWhiteSpace(Branch)
        ? BusinessName
        : $"{BusinessName} - {Branch}";

    public override string ToString() => FullName;
}