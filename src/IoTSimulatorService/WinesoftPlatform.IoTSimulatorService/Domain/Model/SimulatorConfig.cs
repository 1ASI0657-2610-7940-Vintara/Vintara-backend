namespace WinesoftPlatform.IoTSimulatorService.Domain.Model;

/// <summary>
/// Configuration for a sensor simulator defining normal operating ranges,
/// anomaly bounds, and generation parameters.
/// </summary>
public class SimulatorConfig
{
    /// <summary>Lower bound of normal operating range.</summary>
    public double MinNormal { get; set; }

    /// <summary>Upper bound of normal operating range.</summary>
    public double MaxNormal { get; set; }

    /// <summary>Lower bound for anomaly generation (below normal range).</summary>
    public double MinAnomaly { get; set; }

    /// <summary>Upper bound for anomaly generation (above normal range).</summary>
    public double MaxAnomaly { get; set; }

    /// <summary>Probability of generating an anomalous reading (0.0 to 1.0). Default: 20%.</summary>
    public double AnomalyRate { get; set; } = 0.20;

    /// <summary>Unit of measurement (e.g., °C, %HR, kPa, %).</summary>
    public string Unit { get; set; } = string.Empty;

    /// <summary>Interval in milliseconds between readings. Default: 5 seconds.</summary>
    public int IntervalMs { get; set; } = 5000;
}
