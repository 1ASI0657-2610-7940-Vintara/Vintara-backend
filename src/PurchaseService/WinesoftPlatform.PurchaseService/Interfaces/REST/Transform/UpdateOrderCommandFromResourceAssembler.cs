using WinesoftPlatform.API.Purchase.Domain.Model.Commands;
using WinesoftPlatform.API.Purchase.Interfaces.REST.Resources;

namespace WinesoftPlatform.API.Purchase.Interfaces.REST.Transform;

/// <summary>
///     Assembler to create a <see cref="UpdateOrderCommand"/> from a <see cref="UpdateOrderResource"/>.
/// </summary>
public static class UpdateOrderCommandFromResourceAssembler
{
    /// <summary>
    ///     Converts a <see cref="UpdateOrderResource"/> to a <see cref="UpdateOrderCommand"/>.
    /// </summary>
    /// <param name="id">The order identifier.</param>
    /// <param name="resource">The resource containing update data.</param>
    /// <returns>The update command.</returns>
    public static UpdateOrderCommand ToCommandFromResource(int id, UpdateOrderResource resource)
    {
        return new UpdateOrderCommand(id, resource.ProductId, resource.Supplier, resource.Quantity, resource.Status);
    }
}