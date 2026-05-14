namespace WinesoftPlatform.API.Inventory.Interfaces.REST.Resources;

public record CreateSupplyResource(
    string SupplyName,
    int Quantity,
    string Unit,
    string Supplier,
    decimal Price,
    DateTime Date
    );