using WinesoftPlatform.API.Inventory.Domain.Model.Aggregates;
using WinesoftPlatform.API.Inventory.Domain.Model.Commands;

namespace WinesoftPlatform.API.Inventory.Domain.Services;

public interface ISensorAlertCommandService
{
    Task<SensorAlert?> Handle(CreateSensorAlertCommand command);
    Task<SensorAlert?> Handle(AcknowledgeSensorAlertCommand command);
}
