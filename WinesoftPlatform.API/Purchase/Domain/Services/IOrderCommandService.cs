using WinesoftPlatform.API.Purchase.Domain.Model.Aggregates;
using WinesoftPlatform.API.Purchase.Domain.Model.Commands;

namespace WinesoftPlatform.API.Purchase.Domain.Services;

/// <summary>
///     Represents the order command service.
/// </summary>
public interface IOrderCommandService
{
    /// <summary>
    ///     Handles the create order command.
    /// </summary>
    /// <param name="command">The <see cref="CreateOrderCommand"/> to handle.</param>
    /// <returns>The created <see cref="Order"/> entity.</returns>
    Task<Order?> Handle(CreateOrderCommand command);

    /// <summary>
    ///     Handles the update order command.
    /// </summary>
    /// <param name="command">The <see cref="UpdateOrderCommand"/> to handle.</param>
    /// <returns>The updated <see cref="Order"/> entity.</returns>
    Task<Order?> Handle(UpdateOrderCommand command);

    /// <summary>
    ///     Handles the delete order command.
    /// </summary>
    /// <param name="command">The <see cref="DeleteOrderCommand"/> to handle.</param>
    /// <returns>True if deletion was successful; otherwise, false.</returns>
    Task<bool> Handle(DeleteOrderCommand command);
}