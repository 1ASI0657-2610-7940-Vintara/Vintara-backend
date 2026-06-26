using WinesoftPlatform.API.Inventory.Domain.Model.Aggregates;
using WinesoftPlatform.API.Inventory.Domain.Model.Queries;
using WinesoftPlatform.API.Inventory.Domain.Repositories;
using WinesoftPlatform.API.Inventory.Domain.Services;

namespace WinesoftPlatform.API.Inventory.Application.Internal.QueryServices;

public class SupplyQueryService(
    ISupplyRepository supplyRepository
) : ISupplyQueryService
{
    public async Task<IEnumerable<Supply>> Handle(GetAllSuppliesQuery query)
    {
        return await supplyRepository.ListByOwnerIdAsync(query.OwnerId);
    }

    public async Task<IEnumerable<Supply>> Handle(GetAllInternalSuppliesQuery query)
    {
        return await supplyRepository.ListAsync();
    }

    public async Task<Supply?> Handle(GetSupplyByIdQuery query)
    {
        var supply = await supplyRepository.FindByIdAsync(query.Id);
        if (supply != null && supply.OwnerId != query.OwnerId)
        {
            throw new UnauthorizedAccessException("You do not have permission to view this supply.");
        }
        return supply;
    }
}