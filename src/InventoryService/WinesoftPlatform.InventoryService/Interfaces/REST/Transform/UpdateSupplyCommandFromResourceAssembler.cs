using WinesoftPlatform.API.Inventory.Domain.Model.Commands;
using WinesoftPlatform.API.Inventory.Interfaces.REST.Resources;

namespace WinesoftPlatform.API.Inventory.Interfaces.REST.Transform;

public static class UpdateSupplyCommandFromResourceAssembler
{
    public static UpdateSupplyCommand ToCommandFromResource(UpdateSupplyResource resource)
    {
        return new UpdateSupplyCommand(resource.Id, resource.SupplyName, resource.Quantity, resource.Unit, resource.Supplier, resource.Price, resource.Date);
    }
}