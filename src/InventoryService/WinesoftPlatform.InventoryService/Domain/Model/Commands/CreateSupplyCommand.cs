namespace WinesoftPlatform.API.Inventory.Domain.Model.Commands;

public record CreateSupplyCommand(
    string SupplyName,
    int Quantity,
    string Unit,
    string Supplier, 
    decimal Price,
    DateTime Date
    );