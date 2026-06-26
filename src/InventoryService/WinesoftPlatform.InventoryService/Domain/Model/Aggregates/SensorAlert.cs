namespace WinesoftPlatform.API.Inventory.Domain.Model.Aggregates;

/// <summary>
/// Aggregate root representing a sensor alert received from an IoT device.
/// Persisted in the inventory database for historical analysis and dashboard display.
/// </summary>
public class SensorAlert
{
    public int Id { get; private set; }
    public string DeviceId { get; private set; }
    public string SensorType { get; private set; }
    public double Value { get; private set; }
    public string Unit { get; private set; }
    public DateTime Timestamp { get; private set; }
    public string Status { get; private set; }
    public bool IsAnomaly { get; private set; }
    public bool Acknowledged { get; private set; }
    public DateTime? AcknowledgedAt { get; private set; }
    public int OwnerId { get; private set; }

    /// <summary>Required by EF Core.</summary>
    protected SensorAlert()
    {
        DeviceId = string.Empty;
        SensorType = string.Empty;
        Unit = string.Empty;
        Status = string.Empty;
    }

    public SensorAlert(string deviceId, string sensorType, double value, string unit,
        DateTime timestamp, string status, bool isAnomaly, int ownerId)
    {
        DeviceId = deviceId;
        SensorType = sensorType;
        Value = value;
        Unit = unit;
        Timestamp = timestamp;
        Status = status;
        IsAnomaly = isAnomaly;
        Acknowledged = false;
        AcknowledgedAt = null;
        OwnerId = ownerId;
    }

    /// <summary>Marks this alert as acknowledged/resolved.</summary>
    public void Acknowledge()
    {
        Acknowledged = true;
        AcknowledgedAt = DateTime.UtcNow;
    }
}
