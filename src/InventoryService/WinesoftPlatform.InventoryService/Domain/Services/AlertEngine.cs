using WinesoftPlatform.API.Inventory.Domain.Model.Aggregates;
using WinesoftPlatform.API.Inventory.Domain.Repositories;

namespace WinesoftPlatform.API.Inventory.Domain.Services;

public class AlertEngine(ISensorAlertRepository sensorAlertRepository) : IInventoryObserver
{
    public async Task<SensorAlert?> OnSensorReadingReceivedAsync(string deviceId, string sensorType, double value, string unit, DateTime timestamp)
    {
        var status = "NORMAL";
        var isAnomaly = false;

        switch (sensorType.ToLowerInvariant())
        {
            case "temperature":
                if (value < 10.0 || value > 30.0)
                {
                    status = "CRITICAL";
                    isAnomaly = true;
                }
                else if (value < 15.0 || value > 25.0)
                {
                    status = "WARNING";
                    isAnomaly = true;
                }
                break;

            case "humidity":
                if (value < 30.0 || value > 75.0)
                {
                    status = "CRITICAL";
                    isAnomaly = true;
                }
                else if (value < 40.0 || value > 60.0)
                {
                    status = "WARNING";
                    isAnomaly = true;
                }
                break;

            case "pressure":
                if (value < 95.0 || value > 120.0)
                {
                    status = "CRITICAL";
                    isAnomaly = true;
                }
                else if (value < 100.0 || value > 110.0)
                {
                    status = "WARNING";
                    isAnomaly = true;
                }
                break;

            case "level":
                if (value < 15.0)
                {
                    status = "CRITICAL";
                    isAnomaly = true;
                }
                else if (value < 20.0)
                {
                    status = "WARNING";
                    isAnomaly = true;
                }
                break;
        }

        var alert = new SensorAlert(deviceId, sensorType, value, unit, timestamp, status, isAnomaly);
        await sensorAlertRepository.AddAsync(alert);
        return alert;
    }

    public async Task<SensorAlert?> OnSupplyStockChangedAsync(int supplyId, string supplyName, int newQuantity, string unit)
    {
        var status = "NORMAL";
        var isAnomaly = false;

        if (newQuantity < 10)
        {
            status = "CRITICAL";
            isAnomaly = true;
        }
        else if (newQuantity < 20)
        {
            status = "WARNING";
            isAnomaly = true;
        }

        var deviceId = $"SUPPLY-{supplyId}";
        
        var alert = new SensorAlert(
            deviceId,
            "stock",
            newQuantity,
            unit,
            DateTime.UtcNow,
            status,
            isAnomaly
        );

        await sensorAlertRepository.AddAsync(alert);
        return alert;
    }
}
