using WinesoftPlatform.API.Inventory.Domain.Model.Aggregates;
using WinesoftPlatform.API.Inventory.Domain.Model.Queries;

namespace WinesoftPlatform.API.Inventory.Domain.Services;

public interface IStockMovementQueryService
{
    Task<IEnumerable<StockMovement>> Handle(GetAllStockMovementsQuery query);
}
