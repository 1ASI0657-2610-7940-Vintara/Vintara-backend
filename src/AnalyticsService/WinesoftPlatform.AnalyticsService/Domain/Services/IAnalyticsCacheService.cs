namespace WinesoftPlatform.API.Analytics.Domain.Services;

public interface IAnalyticsCacheService
{
    Task TrackCacheKeyAsync(int ownerId, string key);
    Task InvalidateCacheAsync(int ownerId);
}
