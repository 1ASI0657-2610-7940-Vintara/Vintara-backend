using WinesoftPlatform.API.Purchase.Domain.Model.Commands;
using WinesoftPlatform.API.Purchase.Interfaces.REST.Resources;

namespace WinesoftPlatform.API.Purchase.Interfaces.REST.Transform;

/// <summary>
///     Assembler to create a <see cref="CreateOrderCommand"/> from a <see cref="CreateOrderResource"/>.
/// </summary>
public static class CreateOrderCommandFromResourceAssembler
{
    /// <summary>
    ///     Converts a <see cref="CreateOrderResource"/> to a <see cref="CreateOrderCommand"/>.
    /// </summary>
    /// <param name="resource">The resource to convert.</param>
    /// <returns>The created command.</returns>
    public static CreateOrderCommand ToCommandFromResource(CreateOrderResource resource)
    {
        return new CreateOrderCommand(resource.ProductId, resource.Supplier, resource.Quantity, resource.Status);
    }
}