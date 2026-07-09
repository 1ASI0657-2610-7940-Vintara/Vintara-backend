using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json;
using WinesoftPlatform.API.Analytics.Domain.Model.Queries;
using WinesoftPlatform.API.Analytics.Domain.Model.ValueObjects;
using WinesoftPlatform.API.Analytics.Domain.Repositories;
using WinesoftPlatform.API.Analytics.Domain.Services;

namespace WinesoftPlatform.API.Analytics.Application.Internal.QueryServices;

/// <summary>
/// Query service for analytics operations with distributed caching.
/// </summary>
public class AnalyticsQueryService(
    IAnalyticsRepository analyticsRepository,
    IDistributedCache cache,
    IAnalyticsCacheService cacheService) : IAnalyticsQueryService
{
    private async Task<T> GetOrAddAsync<T>(string key, int ownerId, Func<Task<T>> factory)
    {
        var cached = await cache.GetStringAsync(key);
        if (cached != null)
        {
            try
            {
                var deserialized = JsonSerializer.Deserialize<T>(cached);
                if (deserialized != null)
                {
                    return deserialized;
                }
            }
            catch
            {
                // Fallback to factory if deserialization fails
            }
        }

        var result = await factory();
        
        try
        {
            var options = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(Random.Shared.Next(5, 11)) // 5 to 10 minutes
            };
            await cache.SetStringAsync(key, JsonSerializer.Serialize(result), options);
            await cacheService.TrackCacheKeyAsync(ownerId, key);
        }
        catch
        {
            // Ignore cache write errors to keep query service working
        }

        return result;
    }

    /// <inheritdoc />
    public async Task<IEnumerable<SupplyLevel>> Handle(GetAllSupplyLevelsQuery query)
    {
        var key = $"analytics:{query.OwnerId}:supply-levels";
        return await GetOrAddAsync(key, query.OwnerId, () => analyticsRepository.GetSupplyLevelsAsync(query.OwnerId));
    }
    
    /// <inheritdoc />
    public async Task<IEnumerable<LowStockAlert>> Handle(GetLowStockAlertsQuery query)
    {
        var key = $"analytics:{query.OwnerId}:low-stock-alerts:{query.Threshold}";
        return await GetOrAddAsync(key, query.OwnerId, () => analyticsRepository.GetLowStockAlertsAsync(query.OwnerId, query.Threshold));
    }

    /// <inheritdoc />
    public async Task<IEnumerable<SupplyRotationMetric>> Handle(GetSupplyRotationQuery query)
    {
        var endDate = query.EndDate ?? DateTime.UtcNow;
        var startDate = query.StartDate ?? endDate.AddDays(-7);
        var key = $"analytics:{query.OwnerId}:supply-rotation:{startDate:yyyyMMdd}:{endDate:yyyyMMdd}";
        
        return await GetOrAddAsync(key, query.OwnerId, () => analyticsRepository.GetSupplyRotationAsync(query.OwnerId, startDate, endDate));
    }
}