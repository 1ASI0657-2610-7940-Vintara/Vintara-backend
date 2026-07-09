namespace WinesoftPlatform.API.Inventory.Interfaces.REST.Resources;

public record StockMovementResource(
    int Id,
    int SupplyId,
    string SupplyName,
    int Quantity,
    string Type,
    string Reason,
    DateTime Date,
    int OwnerId
);
