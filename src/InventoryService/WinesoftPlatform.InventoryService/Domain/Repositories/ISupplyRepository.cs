using WinesoftPlatform.API.Inventory.Domain.Model.Aggregates;
using WinesoftPlatform.API.Shared.Domain.Repositories;

namespace WinesoftPlatform.API.Inventory.Domain.Repositories;

public interface ISupplyRepository : IBaseRepository<Supply>
{
    Task<Supply?> FindByNameAndSupplierAndOwnerIdAsync(string name, string supplier, int ownerId);
    Task<IEnumerable<Supply>> ListByOwnerIdAsync(int ownerId);
}