using WinesoftPlatform.API.Inventory.Domain.Model.Aggregates;

namespace WinesoftPlatform.API.Inventory.Domain.Services;

public interface IInventorySubject
{
    void RegisterObserver(IInventoryObserver observer);
    void RemoveObserver(IInventoryObserver observer);
    Task<IEnumerable<SensorAlert>> NotifySensorReadingAsync(string deviceId, string sensorType, double value, string unit, DateTime timestamp, int ownerId);
    Task<IEnumerable<SensorAlert>> NotifySupplyStockChangedAsync(int supplyId, string supplyName, int newQuantity, string unit, int ownerId);
}
