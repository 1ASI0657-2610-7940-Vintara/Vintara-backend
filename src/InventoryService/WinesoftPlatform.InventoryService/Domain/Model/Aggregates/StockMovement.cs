using WinesoftPlatform.API.Inventory.Domain.Model.Commands;

namespace WinesoftPlatform.API.Inventory.Domain.Model.Aggregates;

/// <summary>
/// Aggregate root representing a stock movement (entry or exit) in the inventory.
/// </summary>
public class StockMovement
{
    public int Id { get; private set; }
    public int SupplyId { get; private set; }
    public int Quantity { get; private set; }
    public string Type { get; private set; } // "ENTRY" or "EXIT"
    public string Reason { get; private set; }
    public DateTime Date { get; private set; }
    public int OwnerId { get; private set; }

    // Navigation property (optional, managed by EF)
    public Supply? Supply { get; private set; }

    protected StockMovement()
    {
        Type = string.Empty;
        Reason = string.Empty;
    }

    public StockMovement(CreateStockMovementCommand command)
    {
        SupplyId = command.SupplyId;
        Quantity = command.Quantity;
        Type = command.Type.ToUpper();
        Reason = command.Reason ?? string.Empty;
        Date = command.Date;
        OwnerId = command.OwnerId;
    }
}
