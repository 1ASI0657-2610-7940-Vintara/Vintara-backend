using WinesoftPlatform.IoTSimulatorService.Domain.Model;

namespace WinesoftPlatform.IoTSimulatorService.Domain.Interfaces;

/// <summary>
/// Contract for all IoT device simulators.
/// Each implementation generates readings specific to its sensor type.
/// </summary>
public interface IDeviceSimulator
{
    /// <summary>Unique identifier for this device instance.</summary>
    string DeviceId { get; }

    /// <summary>Type of sensor (temperature, humidity, pressure, level).</summary>
    string DeviceType { get; }

    /// <summary>Generates a single telemetry reading with realistic noise and anomaly injection.</summary>
    SensorReading GenerateReading(int quantity, int ownerId);
}
