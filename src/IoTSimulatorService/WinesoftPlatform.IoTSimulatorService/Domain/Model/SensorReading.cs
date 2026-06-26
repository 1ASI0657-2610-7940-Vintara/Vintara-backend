namespace WinesoftPlatform.IoTSimulatorService.Domain.Model;

/// <summary>
/// Represents a single telemetry reading from a simulated IoT sensor device.
/// Immutable by design — each reading is a snapshot in time.
/// </summary>
public record SensorReading(
    string DeviceId,
    string SensorType,
    double Value,
    string Unit,
    DateTime Timestamp,
    string Status,
    bool IsAnomaly,
    int OwnerId
);
