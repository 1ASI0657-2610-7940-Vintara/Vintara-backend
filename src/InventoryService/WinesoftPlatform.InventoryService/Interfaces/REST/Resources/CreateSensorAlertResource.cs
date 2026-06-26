namespace WinesoftPlatform.API.Inventory.Interfaces.REST.Resources;

public record CreateSensorAlertResource(
    string DeviceId,
    string SensorType,
    double Value,
    string Unit,
    DateTime Timestamp,
    string Status,
    bool IsAnomaly,
    int OwnerId
);
