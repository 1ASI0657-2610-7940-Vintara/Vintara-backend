using WinesoftPlatform.API.Analytics.Domain.Model.ValueObjects;
using WinesoftPlatform.API.Analytics.Interfaces.REST.Resources;

namespace WinesoftPlatform.API.Analytics.Interfaces.REST.Transform;

public static class SupplyRotationResourceFromEntityAssembler
{
    public static SupplyRotationResource ToResourceFromEntity(SupplyRotationMetric entity)
    {
        return new SupplyRotationResource(entity.Day, entity.Movements);
    }
}