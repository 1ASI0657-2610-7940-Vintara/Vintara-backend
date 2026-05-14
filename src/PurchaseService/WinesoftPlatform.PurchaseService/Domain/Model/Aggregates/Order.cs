using EntityFrameworkCore.CreatedUpdatedDate.Contracts;
using WinesoftPlatform.API.Purchase.Domain.Model.Commands;

namespace WinesoftPlatform.API.Purchase.Domain.Model.Aggregates;

/// <summary>
///     Order Aggregate Root.
/// </summary>
public partial class Order : IEntityWithCreatedUpdatedDate
{
    public Order()
    {
        Supplier = string.Empty;
        Status = string.Empty;
        SupplyName = string.Empty;
    }

    public Order(CreateOrderCommand command, string supplyName)
    {
        ProductId = command.ProductId;
        SupplyName = supplyName;
        Supplier = command.Supplier;
        Quantity = command.Quantity;
        Status = command.Status;
    }

    public int Id { get; }
    public int ProductId { get; private set; }
    public string SupplyName { get; private set; }
    public string Supplier { get; private set; }
    public int Quantity { get; private set; }
    public string Status { get; private set; }

    public DateTimeOffset? CreatedDate { get; set; }
    public DateTimeOffset? UpdatedDate { get; set; }

    public void UpdateDetails(int productId, string supplyName, string supplier, int quantity, string status)
    {
        ProductId = productId;
        SupplyName = supplyName;
        Supplier = supplier;
        Quantity = quantity;
        Status = status;
    }
}