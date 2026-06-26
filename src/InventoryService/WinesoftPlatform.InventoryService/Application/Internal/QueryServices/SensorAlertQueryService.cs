using WinesoftPlatform.API.Inventory.Domain.Model.Aggregates;
using WinesoftPlatform.API.Inventory.Domain.Model.Queries;
using WinesoftPlatform.API.Inventory.Domain.Repositories;
using WinesoftPlatform.API.Inventory.Domain.Services;

namespace WinesoftPlatform.API.Inventory.Application.Internal.QueryServices;

public class SensorAlertQueryService(
    ISensorAlertRepository sensorAlertRepository
) : ISensorAlertQueryService
{
    public async Task<(IEnumerable<SensorAlert> Items, int TotalItems)> Handle(GetAllSensorAlertsQuery query)
    {
        var items = await sensorAlertRepository.FindAllAsync(
            query.OwnerId, query.Status, query.SensorType, query.Page, query.Size);

        var totalItems = await sensorAlertRepository.CountAsync(
            query.OwnerId, query.Status, query.SensorType);

        return (items, totalItems);
    }

    public async Task<SensorAlert?> Handle(GetSensorAlertByIdQuery query)
    {
        var alert = await sensorAlertRepository.FindByIdAsync(query.Id);
        if (alert != null && alert.OwnerId != query.OwnerId)
        {
            throw new UnauthorizedAccessException("You do not have permission to view this sensor alert.");
        }
        return alert;
    }
}
