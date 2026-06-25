using WinesoftPlatform.IoTSimulatorService.Domain.Interfaces;

namespace WinesoftPlatform.IoTSimulatorService.Application.Simulators;

/// <summary>
/// Factory Method pattern — creates device simulators based on sensor type string.
/// Used by the SimulationEngine to instantiate devices from configuration.
/// </summary>
public static class DeviceSimulatorFactory
{
    public static IDeviceSimulator Create(string type, string deviceId)
        => type.ToLowerInvariant() switch
        {
            "temperature" => new TemperatureSensorSimulator(deviceId),
            "humidity" => new HumiditySensorSimulator(deviceId),
            "pressure" => new PressureSensorSimulator(deviceId),
            "level" => new LevelSensorSimulator(deviceId),
            _ => throw new ArgumentException($"Unknown sensor type: '{type}'. " +
                                             "Valid types: temperature, humidity, pressure, level.")
        };
}
