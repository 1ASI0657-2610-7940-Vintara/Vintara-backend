using WinesoftPlatform.API.Profiles.Domain.Model.Aggregates;
using WinesoftPlatform.API.Profiles.Interfaces.REST.Resources;

namespace WinesoftPlatform.API.Profiles.Interfaces.REST.Transform;

public static class ProfileResourceFromEntityAssembler
{
    public static ProfileResource ToResourceFromEntity(Profile entity)
    {
        return new ProfileResource(entity.Id, entity.FullName, entity.FiscalAddress, entity.ContactPhone ,entity.TaxIdentity);
    }
}