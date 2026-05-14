namespace WinesoftPlatform.API.Purchase.Domain.Model.Queries;

/// <summary>
///     Query to get an order by its unique identifier.
/// </summary>
/// <param name="Id">The order identifier.</param>
public record GetOrderByIdQuery(int Id);