namespace WinesoftPlatform.API.Purchase.Domain.Model.Commands;

/// <summary>
///     Command to update an existing order.
/// </summary>
/// <param name="Id">The identifier of the order to update.</param>
/// <param name="ProductId">The new product identifier.</param>
/// <param name="Supplier">The new supplier name.</param>
/// <param name="Quantity">The new quantity.</param>
/// <param name="Status">The new status.</param>
public record UpdateOrderCommand(int Id, int ProductId, string Supplier, int Quantity, string Status);