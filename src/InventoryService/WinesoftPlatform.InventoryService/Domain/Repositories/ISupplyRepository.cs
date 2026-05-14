using WinesoftPlatform.API.Inventory.Domain.Model.Aggregates;
using WinesoftPlatform.API.Shared.Domain.Repositories;

namespace WinesoftPlatform.API.Inventory.Domain.Repositories;

public interface ISupplyRepository : IBaseRepository<Supply>
{
    Task<Supply?> FindByNameAsync(string name);
    Task<bool> ExistsByNameAsync(string name);
    Task<Supply?> FindByNameAndSupplierAsync(string name, string supplier);
}