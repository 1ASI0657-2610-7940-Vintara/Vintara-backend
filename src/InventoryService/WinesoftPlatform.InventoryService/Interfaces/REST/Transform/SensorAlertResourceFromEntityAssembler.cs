using WinesoftPlatform.API.Inventory.Domain.Model.Aggregates;
using WinesoftPlatform.API.Inventory.Interfaces.REST.Resources;

namespace WinesoftPlatform.API.Inventory.Interfaces.REST.Transform;

public static class SensorAlertResourceFromEntityAssembler
{
    public static SensorAlertResource ToResourceFromEntity(SensorAlert entity)
        => new(
            entity.Id,
            entity.DeviceId,
            entity.SensorType,
            entity.Value,
            entity.Unit,
            entity.Timestamp,
            entity.Status,
            entity.IsAnomaly,
            entity.Acknowledged,
            entity.AcknowledgedAt,
            entity.OwnerId
        );
}
