using WinesoftPlatform.API.Purchase.Application.Internal.CommandServices;
using WinesoftPlatform.API.Purchase.Application.Internal.QueryServices;
using WinesoftPlatform.API.Purchase.Domain.Repositories;
using WinesoftPlatform.API.Purchase.Domain.Services;
using WinesoftPlatform.API.Purchase.Infrastructure.Persistence.EFC.Repositories;

namespace WinesoftPlatform.API.Purchase.Infrastructure.Interfaces.ASP.Configuration.Extensions;

/// <summary>
///     Extension methods for configuring Purchase context services in a WebApplicationBuilder.
/// </summary>
public static class WebApplicationBuilderExtensions
{
    /// <summary>
    ///     Adds the Purchase context services to the WebApplicationBuilder.
    /// </summary>
    /// <param name="builder">The WebApplicationBuilder to configure.</param>
    public static void AddPurchaseContextServices(this WebApplicationBuilder builder)
    {
        builder.Services.AddScoped<IOrderRepository, OrderRepository>();
        builder.Services.AddScoped<IOrderCommandService, OrderCommandService>();
        builder.Services.AddScoped<IOrderQueryService, OrderQueryService>();
    }
}