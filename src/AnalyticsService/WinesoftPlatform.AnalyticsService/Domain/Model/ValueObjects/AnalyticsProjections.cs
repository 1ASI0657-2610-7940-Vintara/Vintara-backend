namespace WinesoftPlatform.API.Analytics.Domain.Model.ValueObjects;

public record SupplyLevel(string SupplyName, int Quantity);
public record LowStockAlert(string SupplyName, int Quantity, int Threshold);
public record SupplyRotationMetric(DateTime Day, int Movements);
