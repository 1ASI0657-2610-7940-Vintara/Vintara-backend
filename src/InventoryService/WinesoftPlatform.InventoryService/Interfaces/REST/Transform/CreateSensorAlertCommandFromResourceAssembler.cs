using WinesoftPlatform.API.Inventory.Domain.Model.Commands;
using WinesoftPlatform.API.Inventory.Interfaces.REST.Resources;

namespace WinesoftPlatform.API.Inventory.Interfaces.REST.Transform;

public static class CreateSensorAlertCommandFromResourceAssembler
{
    public static CreateSensorAlertCommand ToCommandFromResource(CreateSensorAlertResource resource)
        => new(
            resource.DeviceId,
            resource.SensorType,
            resource.Value,
            resource.Unit,
            resource.Timestamp,
            resource.Status,
            resource.IsAnomaly
        );
}
