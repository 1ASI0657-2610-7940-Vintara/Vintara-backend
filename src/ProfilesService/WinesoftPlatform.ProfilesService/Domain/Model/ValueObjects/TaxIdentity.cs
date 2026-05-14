namespace WinesoftPlatform.API.Profiles.Domain.Model.ValueObjects;

public record TaxIdentity(string Number)
{
    public TaxIdentity() : this(string.Empty) { }
    // TODO: Add validation
}