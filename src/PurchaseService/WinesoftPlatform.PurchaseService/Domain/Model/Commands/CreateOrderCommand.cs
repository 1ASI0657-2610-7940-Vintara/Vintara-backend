namespace WinesoftPlatform.API.Purchase.Domain.Model.Commands;

/// <summary>
///     Command to create a new order.
/// </summary>
/// <param name="ProductId">The identifier of the product to order.</param>
/// <param name="Supplier">The name of the supplier.</param>
/// <param name="Quantity">The quantity to order.</param>
/// <param name="Status">The initial status of the order.</param>
public record CreateOrderCommand(int ProductId, string Supplier, int Quantity, string Status);