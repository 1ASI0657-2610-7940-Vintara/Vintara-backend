namespace WinesoftPlatform.API.Purchase.Interfaces.REST.Resources;

public record OrderResource(
    int Id,
    int ProductId,
    string ProductName,
    string Supplier,
    int Quantity,
    string Status,
    DateTimeOffset? CreatedDate
);