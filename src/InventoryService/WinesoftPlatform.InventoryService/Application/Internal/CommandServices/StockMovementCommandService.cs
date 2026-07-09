using WinesoftPlatform.API.Inventory.Domain.Model.Aggregates;
using WinesoftPlatform.API.Inventory.Domain.Model.Commands;
using WinesoftPlatform.API.Inventory.Domain.Repositories;
using WinesoftPlatform.API.Inventory.Domain.Services;
using WinesoftPlatform.API.Shared.Domain.Repositories;

namespace WinesoftPlatform.API.Inventory.Application.Internal.CommandServices;

public class StockMovementCommandService(
    IStockMovementRepository stockMovementRepository,
    ISupplyRepository supplyRepository,
    IInventorySubject inventorySubject,
    IUnitOfWork unitOfWork
) : IStockMovementCommandService
{
    public async Task<StockMovement?> Handle(CreateStockMovementCommand command)
    {
        var supply = await supplyRepository.FindByIdAsync(command.SupplyId);
        if (supply is null)
            throw new KeyNotFoundException("Supply not found.");

        if (supply.OwnerId != command.OwnerId)
            throw new UnauthorizedAccessException("You do not have permission to modify this supply's stock.");

        // Apply stock adjustment based on type
        var movementType = command.Type.ToUpper();
        if (movementType == "ENTRY")
        {
            supply.AddStock(command.Quantity);
        }
        else if (movementType == "EXIT")
        {
            if (supply.Quantity < command.Quantity)
            {
                throw new InvalidOperationException("Insufficient stock for this withdrawal.");
            }
            supply.DeductStock(command.Quantity);
        }
        else
        {
            throw new ArgumentException("Invalid stock movement type. Must be 'ENTRY' or 'EXIT'.");
        }

        var stockMovement = new StockMovement(command);

        try
        {
            // Add movement record
            await stockMovementRepository.AddAsync(stockMovement);

            // Update supply stock
            supplyRepository.Update(supply);
            await unitOfWork.CompleteAsync();

            // Notify stock changed to observers/rabbitmq
            await inventorySubject.NotifySupplyStockChangedAsync(supply.Id, supply.SupplyName, supply.Quantity, supply.Unit, supply.OwnerId);
            await unitOfWork.CompleteAsync();

            return stockMovement;
        }
        catch (Exception e)
        {
            Console.WriteLine($"[CreateStockMovement] Error: {e.Message}");
            return null;
        }
    }
}
