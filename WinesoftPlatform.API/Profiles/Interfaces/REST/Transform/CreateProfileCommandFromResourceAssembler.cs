using WinesoftPlatform.API.Profiles.Domain.Model.Commands;
using WinesoftPlatform.API.Profiles.Interfaces.REST.Resources;

namespace WinesoftPlatform.API.Profiles.Interfaces.REST.Transform;

public static class CreateProfileCommandFromResourceAssembler
{
    public static CreateProfileCommand ToCommandFromResource(CreateProfileResource resource)
    {
        return new CreateProfileCommand(
            resource.BusinessName,
            resource.Branch,
            resource.Street,
            resource.Number,
            resource.City,
            resource.PostalCode,
            resource.Country,
            resource.Phone,
            resource.LegalId
            );
    }
}