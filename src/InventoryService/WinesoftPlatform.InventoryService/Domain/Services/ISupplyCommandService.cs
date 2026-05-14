using WinesoftPlatform.API.Inventory.Domain.Model.Aggregates;
using WinesoftPlatform.API.Inventory.Domain.Model.Commands;

namespace WinesoftPlatform.API.Inventory.Domain.Services;

public interface ISupplyCommandService
{
    Task<Supply?> Handle(CreateSupplyCommand command);
    Task<Supply?> Handle(UpdateSupplyCommand command);
    Task<bool> Handle(DeleteSupplyCommand command);
}