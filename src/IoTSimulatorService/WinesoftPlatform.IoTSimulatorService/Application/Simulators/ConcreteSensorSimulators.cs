using WinesoftPlatform.IoTSimulatorService.Domain.Model;

namespace WinesoftPlatform.IoTSimulatorService.Application.Simulators;

/// <summary>
/// Simulates temperature sensors in wine fermentation tanks.
/// Normal range: 15–25 °C | Anomaly range: 2–15 or 25–40 °C
/// </summary>
public class TemperatureSensorSimulator : BaseDeviceSimulator
{
    public TemperatureSensorSimulator(string deviceId)
        : base(deviceId, "temperature", new SimulatorConfig
        {
            MinNormal = 15.0,
            MaxNormal = 25.0,
            MinAnomaly = 2.0,
            MaxAnomaly = 40.0,
            Unit = "°C",
            AnomalyRate = 0.20
        })
    { }
}

/// <summary>
/// Simulates humidity sensors in warehouse storage areas.
/// Normal range: 40–60 %HR | Anomaly range: 15–40 or 60–90 %HR
/// </summary>
public class HumiditySensorSimulator : BaseDeviceSimulator
{
    public HumiditySensorSimulator(string deviceId)
        : base(deviceId, "humidity", new SimulatorConfig
        {
            MinNormal = 40.0,
            MaxNormal = 60.0,
            MinAnomaly = 15.0,
            MaxAnomaly = 90.0,
            Unit = "%HR",
            AnomalyRate = 0.20
        })
    { }
}

/// <summary>
/// Simulates pressure sensors in fermentation tanks.
/// Normal range: 100–110 kPa | Anomaly range: 85–100 or 110–130 kPa
/// </summary>
public class PressureSensorSimulator : BaseDeviceSimulator
{
    public PressureSensorSimulator(string deviceId)
        : base(deviceId, "pressure", new SimulatorConfig
        {
            MinNormal = 100.0,
            MaxNormal = 110.0,
            MinAnomaly = 85.0,
            MaxAnomaly = 130.0,
            Unit = "kPa",
            AnomalyRate = 0.20
        })
    { }
}

/// <summary>
/// Simulates liquid level sensors in tanks and shelves.
/// Normal range: 20–100 % | Anomaly range: 0–20 or 100–105 %
/// </summary>
public class LevelSensorSimulator : BaseDeviceSimulator
{
    public LevelSensorSimulator(string deviceId)
        : base(deviceId, "level", new SimulatorConfig
        {
            MinNormal = 20.0,
            MaxNormal = 100.0,
            MinAnomaly = 0.0,
            MaxAnomaly = 105.0,
            Unit = "%",
            AnomalyRate = 0.20
        })
    { }
}
