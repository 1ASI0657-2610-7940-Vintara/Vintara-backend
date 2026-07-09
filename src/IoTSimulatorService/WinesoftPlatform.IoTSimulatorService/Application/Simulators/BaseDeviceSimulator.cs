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
    protected double? LastValue;

    public string DeviceId { get; }
    public string DeviceType { get; }

    protected BaseDeviceSimulator(string deviceId, string deviceType, SimulatorConfig config)
    {
        DeviceId = deviceId;
        DeviceType = deviceType;
        Config = config;
    }

    public virtual SensorReading GenerateReading(int quantity, int ownerId)
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
            value = CalculateValue(quantity);
            status = "NORMAL";
            LastValue = value;
        }

        return new SensorReading(
            DeviceId,
            DeviceType,
            Math.Round(value, 2),
            Config.Unit,
            DateTime.UtcNow,
            status,
            isAnomaly,
            ownerId
        );
    }

    protected virtual double CalculateValue(int quantity)
    {
        double baseVal = LastValue ?? (Config.MinNormal + (Config.MaxNormal - Config.MinNormal) / 2);
        // Small random walk step (max 5% of range per cycle)
        double stepRange = (Config.MaxNormal - Config.MinNormal) * 0.05;
        double step = (nextRandomDouble() - 0.5) * stepRange; 
        double value = baseVal + step;
        return Math.Clamp(value, Config.MinNormal, Config.MaxNormal);
    }

    protected double RandomInRange(double min, double max)
        => min + _rng.NextDouble() * (max - min);

    private double nextRandomDouble()
    {
        lock (_rng)
        {
            return _rng.NextDouble();
        }
    }
}
