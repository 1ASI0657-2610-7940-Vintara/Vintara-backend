namespace WinesoftPlatform.API.Profiles.Domain.Model.ValueObjects;

/// <summary>
///     Value object for fiscal address
/// </summary>
/// <param name="Street">
///     The street name
/// </param>
/// <param name="Number">
///     The street number
/// </param>
/// <param name="City">
///     The city name
/// </param>
/// <param name="PostalCode">
///     The postal code
/// </param>
/// <param name="Country">
///     The country name
/// </param>
public record FiscalAddress(string Street, string Number, string City, string PostalCode, string Country)
{
    public FiscalAddress() : this(string.Empty, string.Empty, string.Empty, string.Empty, string.Empty)
    {
    }

    public FiscalAddress(string street) : this(street, string.Empty, string.Empty, string.Empty, string.Empty)
    {
    }

    public FiscalAddress(string street, string number, string city) : this(street, number, city, string.Empty,
        string.Empty)
    {
    }

    public FiscalAddress(string street, string number, string city, string postalCode) : this(street, number, city,
        postalCode, string.Empty)
    {
    }

    public string FullAddress => $"{Street} {Number}, {City}, {PostalCode}, {Country}";
}