using MassTransit;
using Microsoft.Extensions.Logging;
using WinesoftPlatform.API.Analytics.Domain.Services;
using WinesoftPlatform.Shared.Domain.Events;

namespace WinesoftPlatform.AnalyticsService.Application.Internal.Consumers;

public class OrderCreatedConsumer(
    IAnalyticsCacheService cacheService,
    ILogger<OrderCreatedConsumer> logger
) : IConsumer<OrderCreated>
{
    public async Task Consume(ConsumeContext<OrderCreated> context)
    {
        var correlationId = context.CorrelationId?.ToString() ?? Guid.NewGuid().ToString();

        using (Serilog.Context.LogContext.PushProperty("CorrelationId", correlationId))
        {
            logger.LogInformation("Processing OrderCreated event for OrderId: {OrderId}, SupplyId: {SupplyId}, Quantity: {Quantity}, OwnerId: {OwnerId} in AnalyticsService",
                context.Message.OrderId, context.Message.SupplyId, context.Message.Quantity, context.Message.OwnerId);

            await cacheService.InvalidateCacheAsync(context.Message.OwnerId);
            
            logger.LogInformation("Cache successfully invalidated for OwnerId: {OwnerId} due to OrderCreated event", context.Message.OwnerId);
        }
    }
}
