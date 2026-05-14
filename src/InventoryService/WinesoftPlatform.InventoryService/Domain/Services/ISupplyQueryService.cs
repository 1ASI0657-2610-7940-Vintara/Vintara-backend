using WinesoftPlatform.API.Inventory.Domain.Model.Aggregates;
using WinesoftPlatform.API.Inventory.Domain.Model.Queries;

namespace WinesoftPlatform.API.Inventory.Domain.Services;

public interface ISupplyQueryService
{
    Task<IEnumerable<Supply>> Handle(GetAllSuppliesQuery query);
    Task<Supply?> Handle(GetSupplyByIdQuery query);
}