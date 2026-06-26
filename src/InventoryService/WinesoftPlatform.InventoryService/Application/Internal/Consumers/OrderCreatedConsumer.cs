using MassTransit;
using Microsoft.Extensions.Logging;
using WinesoftPlatform.API.Inventory.Domain.Repositories;
using WinesoftPlatform.API.Inventory.Domain.Services;
using WinesoftPlatform.API.Shared.Domain.Repositories;
using WinesoftPlatform.Shared.Domain.Events;

namespace WinesoftPlatform.API.Inventory.Application.Internal.Consumers;

public class OrderCreatedConsumer(
    ISupplyRepository supplyRepository,
    IInventorySubject inventorySubject,
    IUnitOfWork unitOfWork,
    ILogger<OrderCreatedConsumer> logger
) : IConsumer<OrderCreated>
{
    public async Task Consume(ConsumeContext<OrderCreated> context)
    {
        var correlationId = context.CorrelationId?.ToString() ?? Guid.NewGuid().ToString();

        using (Serilog.Context.LogContext.PushProperty("CorrelationId", correlationId))
        {
            logger.LogInformation("Processing OrderCreated event for OrderId: {OrderId}, SupplyId: {SupplyId}, Quantity: {Quantity}",
                context.Message.OrderId, context.Message.SupplyId, context.Message.Quantity);

            var supply = await supplyRepository.FindByIdAsync(context.Message.SupplyId);
            if (supply is null)
            {
                logger.LogWarning("Supply with ID {SupplyId} not found for OrderId: {OrderId}", 
                    context.Message.SupplyId, context.Message.OrderId);
                return;
            }

            // Deduct stock
            supply.DeductStock(context.Message.Quantity);

            try
            {
                supplyRepository.Update(supply);
                await unitOfWork.CompleteAsync();

                // Trigger stock changed observers
                await inventorySubject.NotifySupplyStockChangedAsync(supply.Id, supply.SupplyName, supply.Quantity, supply.Unit, supply.OwnerId);
                await unitOfWork.CompleteAsync();

                logger.LogInformation("Stock updated successfully for SupplyId: {SupplyId}. New Quantity: {Quantity}", 
                    supply.Id, supply.Quantity);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error updating stock for SupplyId: {SupplyId} on OrderCreated event", supply.Id);
                throw;
            }
        }
    }
}
