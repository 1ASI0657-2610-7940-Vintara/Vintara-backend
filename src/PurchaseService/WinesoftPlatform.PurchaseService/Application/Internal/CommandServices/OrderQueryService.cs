using WinesoftPlatform.API.Purchase.Domain.Model.Aggregates;
using WinesoftPlatform.API.Purchase.Domain.Model.Queries;
using WinesoftPlatform.API.Purchase.Domain.Repositories;
using WinesoftPlatform.API.Purchase.Domain.Services;

namespace WinesoftPlatform.API.Purchase.Application.Internal.QueryServices;

/// <summary>
///     Order query service implementation.
/// </summary>
/// <param name="orderRepository">The order repository.</param>
public class OrderQueryService(IOrderRepository orderRepository) : IOrderQueryService
{
    /// <inheritdoc />
    public async Task<IEnumerable<Order>> Handle(GetAllOrdersQuery query)
    {
        return await orderRepository.ListAsync();
    }

    /// <inheritdoc />
    public async Task<Order?> Handle(GetOrderByIdQuery query)
    {
        return await orderRepository.FindByIdAsync(query.Id);
    }
}