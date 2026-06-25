using WinesoftPlatform.IoTSimulatorService.Domain.Interfaces;
using WinesoftPlatform.IoTSimulatorService.Domain.Model;

namespace WinesoftPlatform.IoTSimulatorService.Application.Simulators;

/// <summary>
/// Base class for all device simulators implementing the "Realistic Mode" pattern:
/// - 80% of readings fall within the normal operating range
/// - 20% of readings are anomalies (outliers above or below normal range)
/// </summary>
public abstract class BaseDeviceSimulator : IDeviceSimulator
{
    private readonly Random _rng = new();
    protected readonly SimulatorConfig Config;

    public string DeviceId { get; }
    public string DeviceType { get; }

    protected BaseDeviceSimulator(string deviceId, string deviceType, SimulatorConfig config)
    {
        DeviceId = deviceId;
        DeviceType = deviceType;
        Config = config;
    }

    public SensorReading GenerateReading()
    {
        var isAnomaly = _rng.NextDouble() < Config.AnomalyRate;
        double value;
        string status;

        if (isAnomaly)
        {
            // 50% chance of outlier below range, 50% above range
            value = _rng.NextDouble() < 0.5
                ? RandomInRange(Config.MinAnomaly, Config.MinNormal)
                : RandomInRange(Config.MaxNormal, Config.MaxAnomaly);
            status = "CRITICAL";
        }
        else
        {
            value = RandomInRange(Config.MinNormal, Config.MaxNormal);
            status = "NORMAL";
        }

        return new SensorReading(
            DeviceId,
            DeviceType,
            Math.Round(value, 2),
            Config.Unit,
            DateTime.UtcNow,
            status,
            isAnomaly
        );
    }

    private double RandomInRange(double min, double max)
        => min + _rng.NextDouble() * (max - min);
}
