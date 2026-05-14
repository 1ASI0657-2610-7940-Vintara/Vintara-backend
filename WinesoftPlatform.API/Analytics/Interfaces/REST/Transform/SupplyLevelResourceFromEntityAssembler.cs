using WinesoftPlatform.API.Analytics.Domain.Model.ValueObjects;
using WinesoftPlatform.API.Analytics.Interfaces.REST.Resources;

namespace WinesoftPlatform.API.Analytics.Interfaces.REST.Transform;

public static class SupplyLevelResourceFromEntityAssembler
{
    public static SupplyLevelResource ToResourceFromEntity(SupplyLevel entity)
    {
        return new SupplyLevelResource(entity.SupplyName, entity.Quantity);
    }
}