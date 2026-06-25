namespace WinesoftPlatform.API.Inventory.Domain.Model.Queries;

public record GetAllSensorAlertsQuery(
    string? Status = null,
    string? SensorType = null,
    int Page = 1,
    int Size = 20
);
