using WinesoftPlatform.API.Inventory.Domain.Model.Aggregates;
using WinesoftPlatform.API.Inventory.Interfaces.REST.Resources;

namespace WinesoftPlatform.API.Inventory.Interfaces.REST.Transform;

public static class StockMovementResourceFromEntityAssembler
{
    public static StockMovementResource ToResourceFromEntity(StockMovement entity)
    {
        return new StockMovementResource(
            entity.Id,
            entity.SupplyId,
            entity.Supply?.SupplyName ?? string.Empty,
            entity.Quantity,
            entity.Type,
            entity.Reason,
            entity.Date,
            entity.OwnerId
        );
    }
}
