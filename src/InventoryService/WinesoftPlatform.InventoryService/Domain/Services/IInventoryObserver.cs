using WinesoftPlatform.API.Inventory.Domain.Model.Aggregates;

namespace WinesoftPlatform.API.Inventory.Domain.Services;

public interface IInventoryObserver
{
    Task<SensorAlert?> OnSensorReadingReceivedAsync(string deviceId, string sensorType, double value, string unit, DateTime timestamp);
    Task<SensorAlert?> OnSupplyStockChangedAsync(int supplyId, string supplyName, int newQuantity, string unit);
}
