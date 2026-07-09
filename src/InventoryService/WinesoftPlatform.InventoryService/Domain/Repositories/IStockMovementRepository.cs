using WinesoftPlatform.API.Inventory.Domain.Model.Aggregates;
using WinesoftPlatform.API.Shared.Domain.Repositories;

namespace WinesoftPlatform.API.Inventory.Domain.Repositories;

public interface IStockMovementRepository : IBaseRepository<StockMovement>
{
    Task<IEnumerable<StockMovement>> ListByOwnerIdAsync(int ownerId);
    Task<IEnumerable<StockMovement>> ListBySupplyIdAndOwnerIdAsync(int supplyId, int ownerId);
}
