using WinesoftPlatform.API.Inventory.Domain.Model.Aggregates;
using WinesoftPlatform.API.Inventory.Domain.Model.Commands;
using WinesoftPlatform.API.Inventory.Domain.Repositories;
using WinesoftPlatform.API.Inventory.Domain.Services;
using WinesoftPlatform.API.Shared.Domain.Repositories;

namespace WinesoftPlatform.API.Inventory.Application.Internal.CommandServices;

public class SupplyCommandService(
    ISupplyRepository supplyRepository,
    IInventorySubject inventorySubject,
    IUnitOfWork unitOfWork
) : ISupplyCommandService
{
    public async Task<Supply?> Handle(CreateSupplyCommand command)
    {
        var existingSupply = await supplyRepository.FindByNameAndSupplierAndOwnerIdAsync(command.SupplyName, command.Supplier, command.OwnerId);
        if (existingSupply is not null)
            throw new Exception("Supply already exists for this supplier");
        
        var supply = new Supply(command);

        try
        {
            await supplyRepository.AddAsync(supply);
            await unitOfWork.CompleteAsync();

            await inventorySubject.NotifySupplyStockChangedAsync(supply.Id, supply.SupplyName, supply.Quantity, supply.Unit, supply.OwnerId);
            await unitOfWork.CompleteAsync();

            return supply;
        }
        catch (Exception e)
        {
            Console.WriteLine($"[CreateSupply] Error: {e.Message}");
            return null;
        }
    }

    public async Task<Supply?> Handle(UpdateSupplyCommand command)
    {
        var existing = await supplyRepository.FindByIdAsync(command.Id);
        if (existing is null)
            return null;

        if (existing.OwnerId != command.OwnerId)
            throw new UnauthorizedAccessException("You do not have permission to modify this supply.");
        
        var conflict = await supplyRepository.FindByNameAndSupplierAndOwnerIdAsync(command.SupplyName, command.Supplier, command.OwnerId);
        if (conflict is not null && conflict.Id != existing.Id)
            throw new Exception("Supply already exists for this supplier");

        existing.UpdateDetails(
            command.SupplyName,
            command.Quantity,
            command.Unit,
            command.Supplier,
            command.Price,
            command.Date,
            command.OwnerId
        );

        try
        {
            supplyRepository.Update(existing);
            await unitOfWork.CompleteAsync();

            await inventorySubject.NotifySupplyStockChangedAsync(existing.Id, existing.SupplyName, existing.Quantity, existing.Unit, existing.OwnerId);
            await unitOfWork.CompleteAsync();

            return existing;
        }
        catch (Exception e)
        {
            Console.WriteLine($"[UpdateSupply] Error: {e.Message}");
            return null;
        }
    }

    public async Task<bool> Handle(DeleteSupplyCommand command)
    {
        var existingSupply = await supplyRepository.FindByIdAsync(command.Id);
        if (existingSupply is null)
            return false;

        if (existingSupply.OwnerId != command.OwnerId)
            throw new UnauthorizedAccessException("You do not have permission to delete this supply.");

        try
        {
            supplyRepository.Remove(existingSupply);
            await unitOfWork.CompleteAsync();
            return true;
        }
        catch (Exception e)
        {
            Console.WriteLine($"[Error - DeleteSupply] {e.Message}");
            return false;
        }
    }
}