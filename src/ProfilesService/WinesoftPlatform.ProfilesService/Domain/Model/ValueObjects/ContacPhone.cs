namespace WinesoftPlatform.API.Profiles.Domain.Model.ValueObjects;

public record ContactPhone(string Number)
{
    public ContactPhone() : this(string.Empty)
    {
    }
    
}