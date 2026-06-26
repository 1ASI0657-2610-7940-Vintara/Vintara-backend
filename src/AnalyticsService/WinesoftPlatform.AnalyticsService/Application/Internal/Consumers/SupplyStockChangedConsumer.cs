using MassTransit;
using Microsoft.Extensions.Logging;
using WinesoftPlatform.API.Analytics.Domain.Services;
using WinesoftPlatform.Shared.Domain.Events;

namespace WinesoftPlatform.AnalyticsService.Application.Internal.Consumers;

public class SupplyStockChangedConsumer(
    IAnalyticsCacheService cacheService,
    ILogger<SupplyStockChangedConsumer> logger
) : IConsumer<SupplyStockChanged>
{
    public async Task Consume(ConsumeContext<SupplyStockChanged> context)
    {
        var correlationId = context.CorrelationId?.ToString() ?? Guid.NewGuid().ToString();

        using (Serilog.Context.LogContext.PushProperty("CorrelationId", correlationId))
        {
            logger.LogInformation("Processing SupplyStockChanged event for SupplyId: {SupplyId}, Name: {SupplyName}, NewQty: {Quantity}, OwnerId: {OwnerId} in AnalyticsService",
                context.Message.SupplyId, context.Message.SupplyName, context.Message.NewQuantity, context.Message.OwnerId);

            await cacheService.InvalidateCacheAsync(context.Message.OwnerId);
            
            logger.LogInformation("Cache successfully invalidated for OwnerId: {OwnerId} due to SupplyStockChanged event", context.Message.OwnerId);
        }
    }
}
