namespace WinesoftPlatform.API.Profiles.Domain.Model.Commands;

public record CreateProfileCommand(
    string BusinessName,
    string Branch,
    string Street,
    string Number,
    string City,
    string PostalCode,
    string Country,
    string Phone,
    string LegalId
    );