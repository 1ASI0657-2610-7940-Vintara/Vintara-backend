namespace WinesoftPlatform.API.Inventory.Interfaces.REST.Resources;

public record SensorAlertResource(
    int Id,
    string DeviceId,
    string SensorType,
    double Value,
    string Unit,
    DateTime Timestamp,
    string Status,
    bool IsAnomaly,
    bool Acknowledged,
    DateTime? AcknowledgedAt,
    int OwnerId
);
