namespace WinesoftPlatform.API.Analytics.Domain.Model.ValueObjects;

public record PurchaseOrderSummary(int OrderId, string Status, DateTime Date, int ProductId, int Quantity, string Supplier);
public record SupplyLevel(string SupplyName, int Quantity);
public record LowStockAlert(string SupplyName, int Quantity, int Threshold);
public record SupplyRotationMetric(DateTime Day, int Movements);
public record CostsSummary(double TotalCost, DateTime StartDate, DateTime EndDate);