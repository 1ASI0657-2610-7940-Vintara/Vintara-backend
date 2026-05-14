using WinesoftPlatform.API.Purchase.Domain.Model.Aggregates;
using WinesoftPlatform.API.Purchase.Domain.Model.Queries;

namespace WinesoftPlatform.API.Purchase.Domain.Services;

/// <summary>
///     Represents the order query service.
/// </summary>
public interface IOrderQueryService
{
    /// <summary>
    ///     Handles the get all orders query.
    /// </summary>
    /// <param name="query">The <see cref="GetAllOrdersQuery"/> to handle.</param>
    /// <returns>A collection of <see cref="Order"/> entities.</returns>
    Task<IEnumerable<Order>> Handle(GetAllOrdersQuery query);

    /// <summary>
    ///     Handles the get order by id query.
    /// </summary>
    /// <param name="query">The <see cref="GetOrderByIdQuery"/> to handle.</param>
    /// <returns>The <see cref="Order"/> entity if found; otherwise, null.</returns>
    Task<Order?> Handle(GetOrderByIdQuery query);
}