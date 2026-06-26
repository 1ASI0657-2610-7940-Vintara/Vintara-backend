using WinesoftPlatform.API.Inventory.Domain.Model.Aggregates;
using WinesoftPlatform.API.Inventory.Domain.Services;

namespace WinesoftPlatform.API.Inventory.Application.Internal.CommandServices;

public class InventorySubject(IEnumerable<IInventoryObserver> observers, MassTransit.IPublishEndpoint publishEndpoint) : IInventorySubject
{
    private readonly List<IInventoryObserver> _dynamicObservers = new();

    public void RegisterObserver(IInventoryObserver observer)
    {
        _dynamicObservers.Add(observer);
    }

    public void RemoveObserver(IInventoryObserver observer)
    {
        _dynamicObservers.Remove(observer);
    }

    public async Task<IEnumerable<SensorAlert>> NotifySensorReadingAsync(string deviceId, string sensorType, double value, string unit, DateTime timestamp, int ownerId)
    {
        var createdAlerts = new List<SensorAlert>();
        
        foreach (var observer in observers)
        {
            var alert = await observer.OnSensorReadingReceivedAsync(deviceId, sensorType, value, unit, timestamp, ownerId);
            if (alert is not null) createdAlerts.Add(alert);
        }
        
        foreach (var observer in _dynamicObservers)
        {
            var alert = await observer.OnSensorReadingReceivedAsync(deviceId, sensorType, value, unit, timestamp, ownerId);
            if (alert is not null) createdAlerts.Add(alert);
        }

        return createdAlerts;
    }

    public async Task<IEnumerable<SensorAlert>> NotifySupplyStockChangedAsync(int supplyId, string supplyName, int newQuantity, string unit, int ownerId)
    {
        // Publish SupplyStockChanged to RabbitMQ
        await publishEndpoint.Publish<WinesoftPlatform.Shared.Domain.Events.SupplyStockChanged>(
            new WinesoftPlatform.Shared.Domain.Events.SupplyStockChanged(supplyId, supplyName, newQuantity, unit, ownerId)
        );

        var createdAlerts = new List<SensorAlert>();

        foreach (var observer in observers)
        {
            var alert = await observer.OnSupplyStockChangedAsync(supplyId, supplyName, newQuantity, unit, ownerId);
            if (alert is not null) createdAlerts.Add(alert);
        }

        foreach (var observer in _dynamicObservers)
        {
            var alert = await observer.OnSupplyStockChangedAsync(supplyId, supplyName, newQuantity, unit, ownerId);
            if (alert is not null) createdAlerts.Add(alert);
        }

        return createdAlerts;
    }
}
