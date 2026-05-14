using WinesoftPlatform.API.Inventory.Domain.Model.Aggregates;
using WinesoftPlatform.API.Inventory.Interfaces.REST.Resources;

namespace WinesoftPlatform.API.Inventory.Interfaces.REST.Transform;

public static class SupplyResourceFromEntityAssembler
{
    public static SupplyResource ToResourceFromEntity(Supply entity)
    {
        return new SupplyResource(entity.Id, entity.SupplyName, entity.Quantity, entity.Unit, entity.Supplier, entity.Price, entity.Date);
    }
}