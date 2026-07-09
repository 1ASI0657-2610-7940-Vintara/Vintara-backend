using WinesoftPlatform.API.Inventory.Domain.Model.Commands;
using WinesoftPlatform.API.Inventory.Interfaces.REST.Resources;

namespace WinesoftPlatform.API.Inventory.Interfaces.REST.Transform;

public static class CreateStockMovementCommandFromResourceAssembler
{
    public static CreateStockMovementCommand ToCommandFromResource(CreateStockMovementResource resource, int ownerId)
    {
        return new CreateStockMovementCommand(
            resource.SupplyId,
            resource.Quantity,
            resource.Type,
            resource.Reason,
            resource.Date == default ? DateTime.UtcNow : resource.Date,
            ownerId
        );
    }
}
