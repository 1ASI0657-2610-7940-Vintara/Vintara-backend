using WinesoftPlatform.API.Inventory.Domain.Model.Aggregates;

namespace WinesoftPlatform.API.Inventory.Domain.Repositories;

public interface ISensorAlertRepository
{
    Task AddAsync(SensorAlert alert);
    Task<SensorAlert?> FindByIdAsync(int id);
    Task<IEnumerable<SensorAlert>> FindAllAsync(int ownerId, string? status, string? sensorType, int page, int size);
    Task<int> CountAsync(int ownerId, string? status, string? sensorType);
    void Update(SensorAlert alert);
}
