namespace WinesoftPlatform.API.Inventory.Domain.Model.Commands;

public record CreateStockMovementCommand(
    int SupplyId,
    int Quantity,
    string Type,
    string Reason,
    DateTime Date,
    int OwnerId
);
