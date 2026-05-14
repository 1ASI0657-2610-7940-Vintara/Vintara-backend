namespace WinesoftPlatform.API.Profiles.Interfaces.REST.Resources;

public record CreateProfileResource(
    string BusinessName,
    string Branch,
    string Street,
    string Number,
    string City,
    string PostalCode,
    string Country,
    string Phone,
    string LegalId);