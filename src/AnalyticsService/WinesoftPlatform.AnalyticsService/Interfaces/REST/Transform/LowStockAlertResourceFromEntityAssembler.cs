using WinesoftPlatform.API.Analytics.Domain.Model.ValueObjects;
using WinesoftPlatform.API.Analytics.Interfaces.REST.Resources;

namespace WinesoftPlatform.API.Analytics.Interfaces.REST.Transform;

public static class LowStockAlertResourceFromEntityAssembler
{
    public static LowStockAlertResource ToResourceFromEntity(LowStockAlert entity)
    {
        return new LowStockAlertResource(entity.SupplyName, entity.Quantity, entity.Threshold);
    }
}