using WinesoftPlatform.API.Analytics.Domain.Model.ValueObjects;
using WinesoftPlatform.API.Analytics.Interfaces.REST.Resources;

namespace WinesoftPlatform.API.Analytics.Interfaces.REST.Transform;

public static class PurchaseOrderResourceFromEntityAssembler
{
    public static PurchaseOrderResource ToResourceFromEntity(PurchaseOrderSummary entity)
    {
        return new PurchaseOrderResource(
            entity.OrderId,
            entity.Status,
            entity.Date,
            entity.ProductId,
            entity.Quantity,
            entity.Supplier
        );
    }
}