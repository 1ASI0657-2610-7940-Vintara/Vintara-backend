using WinesoftPlatform.API.Purchase.Domain.Model.Aggregates;
using WinesoftPlatform.API.Purchase.Interfaces.REST.Resources;

namespace WinesoftPlatform.API.Purchase.Interfaces.REST.Transform;

/// <summary>
///     Assembler to convert an <see cref="Order"/> entity to an <see cref="OrderResource"/>.
/// </summary>
public static class OrderResourceFromEntityAssembler
{
    /// <summary>
    ///     Converts an <see cref="Order"/> entity to an <see cref="OrderResource"/>.
    /// </summary>
    /// <param name="entity">The order entity.</param>
    /// <returns>The order resource.</returns>
    public static OrderResource ToResourceFromEntity(Order entity)
    {
        return new OrderResource(
            entity.Id,
            entity.ProductId,
            entity.SupplyName,
            entity.Supplier,
            entity.Quantity,
            entity.Status,
            entity.CreatedDate
        );
    }
}