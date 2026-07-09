using WinesoftPlatform.API.Inventory.Domain.Model.Aggregates;
using WinesoftPlatform.API.Inventory.Domain.Model.Queries;
using WinesoftPlatform.API.Inventory.Domain.Repositories;
using WinesoftPlatform.API.Inventory.Domain.Services;

namespace WinesoftPlatform.API.Inventory.Application.Internal.QueryServices;

public class StockMovementQueryService(
    IStockMovementRepository stockMovementRepository
) : IStockMovementQueryService
{
    public async Task<IEnumerable<StockMovement>> Handle(GetAllStockMovementsQuery query)
    {
        return await stockMovementRepository.ListByOwnerIdAsync(query.OwnerId);
    }
}
