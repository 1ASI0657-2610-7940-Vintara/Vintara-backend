using System.Text.Json;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using WinesoftPlatform.API.Analytics.Domain.Services;

namespace WinesoftPlatform.AnalyticsService.Infrastructure.Services;

public class AnalyticsCacheService(IDistributedCache cache, ILogger<AnalyticsCacheService> logger) : IAnalyticsCacheService
{
    public async Task TrackCacheKeyAsync(int ownerId, string key)
    {
        var trackingKey = $"analytics:{ownerId}:keys";
        try
        {
            var trackingData = await cache.GetStringAsync(trackingKey);
            var keys = trackingData != null
                ? JsonSerializer.Deserialize<HashSet<string>>(trackingData)
                : new HashSet<string>();

            keys ??= new HashSet<string>();

            if (keys.Add(key))
            {
                var options = new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromDays(2)
                };
                await cache.SetStringAsync(trackingKey, JsonSerializer.Serialize(keys), options);
                logger.LogDebug("Tracked cache key '{CacheKey}' for OwnerId {OwnerId}", key, ownerId);
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to track cache key '{CacheKey}' for OwnerId {OwnerId}", key, ownerId);
        }
    }

    public async Task InvalidateCacheAsync(int ownerId)
    {
        var trackingKey = $"analytics:{ownerId}:keys";
        logger.LogInformation("Invalidating cache for OwnerId {OwnerId}", ownerId);

        try
        {
            var trackingData = await cache.GetStringAsync(trackingKey);
            if (trackingData != null)
            {
                var keys = JsonSerializer.Deserialize<HashSet<string>>(trackingData);
                if (keys != null)
                {
                    foreach (var key in keys)
                    {
                        await cache.RemoveAsync(key);
                        logger.LogDebug("Invalidated cache key '{CacheKey}' for OwnerId {OwnerId}", key, ownerId);
                    }
                }
                await cache.RemoveAsync(trackingKey);
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error occurred during cache invalidation for OwnerId {OwnerId}", ownerId);
        }
    }
}
