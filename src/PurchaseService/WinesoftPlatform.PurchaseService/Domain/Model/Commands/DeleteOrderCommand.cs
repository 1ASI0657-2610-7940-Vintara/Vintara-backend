namespace WinesoftPlatform.API.Purchase.Domain.Model.Commands;

/// <summary>
///     Command to delete an order.
/// </summary>
/// <param name="Id">The identifier of the order to delete.</param>
public record DeleteOrderCommand(int Id);