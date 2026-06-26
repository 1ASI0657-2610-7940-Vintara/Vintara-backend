using WinesoftPlatform.API.Inventory.Domain.Model.Aggregates;
using WinesoftPlatform.API.Inventory.Domain.Model.Commands;
using WinesoftPlatform.API.Inventory.Domain.Repositories;
using WinesoftPlatform.API.Inventory.Domain.Services;
using WinesoftPlatform.API.Shared.Domain.Repositories;

namespace WinesoftPlatform.API.Inventory.Application.Internal.CommandServices;

public class SensorAlertCommandService(
    ISensorAlertRepository sensorAlertRepository,
    IInventorySubject inventorySubject,
    IUnitOfWork unitOfWork
) : ISensorAlertCommandService
{
    public async Task<SensorAlert?> Handle(CreateSensorAlertCommand command)
    {
        var alerts = await inventorySubject.NotifySensorReadingAsync(
            command.DeviceId,
            command.SensorType,
            command.Value,
            command.Unit,
            command.Timestamp,
            command.OwnerId
        );

        await unitOfWork.CompleteAsync();

        // Return the alert that matches this device and type directly from the notification result
        return alerts.FirstOrDefault(a => a.DeviceId == command.DeviceId && a.SensorType == command.SensorType);
    }

    public async Task<SensorAlert?> Handle(AcknowledgeSensorAlertCommand command)
    {
        var alert = await sensorAlertRepository.FindByIdAsync(command.Id);
        if (alert is null) return null;

        alert.Acknowledge();
        sensorAlertRepository.Update(alert);
        await unitOfWork.CompleteAsync();
        return alert;
    }
}
