namespace WinesoftPlatform.API.Analytics.Interfaces.REST.Resources;

/// <summary>
/// Resource representing a low stock alert for a specific product.
/// </summary>
/// <param name="ProductName">The name of the product that is running low.</param>
/// <param name="CurrentStock">The current quantity of the product available in inventory.</param>
/// <param name="Threshold">The minimum stock threshold defined for this product. If stock is below this value, an alert is triggered.</param>
public record LowStockAlertResource(
    string ProductName, 
    int CurrentStock, 
    int Threshold
    );