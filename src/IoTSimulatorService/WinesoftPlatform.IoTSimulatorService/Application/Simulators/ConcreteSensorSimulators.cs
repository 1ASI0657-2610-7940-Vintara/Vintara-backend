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

    protected override double CalculateValue(int quantity)
    {
        // Hydrostatic pressure increases with fill level (0 to 100%).
        double level = (quantity / 500.0) * 100.0;
        double levelClamped = Math.Clamp(level, 0.0, 100.0);
        double pressure = 100.0 + (levelClamped * 0.10); // max 110.0
        // Add tiny random walk fluctuations
        double baseVal = LastValue ?? pressure;
        double step = (RandomInRange(0, 1) - 0.5) * 0.3;
        double newVal = baseVal + step;
        // Keep it anchored around the hydrostatic pressure calculated value
        newVal = (newVal * 0.7) + (pressure * 0.3);
        return Math.Clamp(newVal, Config.MinNormal, Config.MaxNormal);
    }
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

    protected override double CalculateValue(int quantity)
    {
        // 500 units is the target capacity. Scale stock quantity to a percentage level.
        double targetLevel = (quantity / 500.0) * 100.0;
        // Add tiny fluctuation (noise)
        double noise = (RandomInRange(0, 1) - 0.5) * 0.2;
        return Math.Clamp(targetLevel + noise, Config.MinNormal, Config.MaxNormal);
    }
}
