using WinesoftPlatform.API.Purchase.Domain.Model.Aggregates;
using WinesoftPlatform.API.Purchase.Domain.Model.Commands;
using WinesoftPlatform.API.Purchase.Domain.Repositories;
using WinesoftPlatform.API.Purchase.Domain.Services;
using WinesoftPlatform.API.Shared.Domain.Repositories;
using WinesoftPlatform.PurchaseService.Infrastructure.ExternalServices;

namespace WinesoftPlatform.API.Purchase.Application.Internal.CommandServices;

/// <summary>
///     Order command service implementation.
/// </summary>
/// <param name="orderRepository">The order repository.</param>
/// <param name="inventoryClient">The inventory service client.</param>
/// <param name="unitOfWork">The unit of work.</param>
public class OrderCommandService(
    IOrderRepository orderRepository,
    IInventoryServiceClient inventoryClient,
    IUnitOfWork unitOfWork)
    : IOrderCommandService
{
    /// <inheritdoc />
    public async Task<Order?> Handle(CreateOrderCommand command)
    {
        var supplyName = await inventoryClient.GetSupplyNameAsync(command.ProductId);
        if (supplyName == "Unknown Supply") throw new Exception("Supply not found");

        var order = new Order(command, supplyName);

        try
        {
            await orderRepository.AddAsync(order);
            await unitOfWork.CompleteAsync();
            return order;
        }
        catch (Exception e)
        {
            Console.WriteLine($"Error creating order: {e.Message}");
            return null;
        }
    }

    /// <inheritdoc />
    public async Task<Order?> Handle(UpdateOrderCommand command)
    {
        var order = await orderRepository.FindByIdAsync(command.Id);
        if (order is null) return null;

        var supplyName = await inventoryClient.GetSupplyNameAsync(command.ProductId);
        if (supplyName == "Unknown Supply") throw new Exception("Supply not found");

        order.UpdateDetails(command.ProductId, supplyName, command.Supplier, command.Quantity, command.Status);

        try
        {
            orderRepository.Update(order);
            await unitOfWork.CompleteAsync();
            return order;
        }
        catch (Exception e)
        {
            Console.WriteLine($"Error updating order: {e.Message}");
            return null;
        }
    }

    /// <inheritdoc />
    public async Task<bool> Handle(DeleteOrderCommand command)
    {
        var order = await orderRepository.FindByIdAsync(command.Id);
        if (order is null) return false;

        try
        {
            orderRepository.Remove(order);
            await unitOfWork.CompleteAsync();
            return true;
        }
        catch (Exception e)
        {
            Console.WriteLine($"Error deleting order: {e.Message}");
            return false;
        }
    }
}