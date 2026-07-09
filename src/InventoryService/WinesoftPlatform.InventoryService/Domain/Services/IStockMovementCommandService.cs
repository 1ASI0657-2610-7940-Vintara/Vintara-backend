using WinesoftPlatform.API.Inventory.Domain.Model.Aggregates;
using WinesoftPlatform.API.Inventory.Domain.Model.Commands;

namespace WinesoftPlatform.API.Inventory.Domain.Services;

public interface IStockMovementCommandService
{
    Task<StockMovement?> Handle(CreateStockMovementCommand command);
}
