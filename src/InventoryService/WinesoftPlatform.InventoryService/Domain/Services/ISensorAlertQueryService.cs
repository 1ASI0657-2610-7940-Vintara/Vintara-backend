using WinesoftPlatform.API.Inventory.Domain.Model.Aggregates;
using WinesoftPlatform.API.Inventory.Domain.Model.Queries;

namespace WinesoftPlatform.API.Inventory.Domain.Services;

public interface ISensorAlertQueryService
{
    Task<(IEnumerable<SensorAlert> Items, int TotalItems)> Handle(GetAllSensorAlertsQuery query);
    Task<SensorAlert?> Handle(GetSensorAlertByIdQuery query);
}
