namespace WinesoftPlatform.API.Inventory.Domain.Model.Commands;

public record CreateSensorAlertCommand(
    string DeviceId,
    string SensorType,
    double Value,
    string Unit,
    DateTime Timestamp,
    string Status,
    bool IsAnomaly,
    int OwnerId
);
