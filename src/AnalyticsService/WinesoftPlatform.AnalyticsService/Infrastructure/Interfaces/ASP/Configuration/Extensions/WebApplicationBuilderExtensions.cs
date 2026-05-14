using WinesoftPlatform.API.Analytics.Application.Internal.CommandServices;
using WinesoftPlatform.API.Analytics.Application.Internal.QueryServices;
using WinesoftPlatform.API.Analytics.Domain.Repositories;
using WinesoftPlatform.API.Analytics.Domain.Services;
using WinesoftPlatform.API.Analytics.Infrastructure.Persistence.Repositories;
using WinesoftPlatform.API.Analytics.Infrastructure.Services;

namespace WinesoftPlatform.API.Analytics.Infrastructure.Interfaces.ASP.Configuration.Extensions;

/// <summary>
/// Extension methods for configuring Analytics context services in a WebApplicationBuilder.
/// </summary>
public static class WebApplicationBuilderExtensions
{
    /// <summary>
    /// Adds the Analytics context services to the WebApplicationBuilder.
    /// </summary>
    /// <param name="builder">The WebApplicationBuilder to configure.</param>

    public static void AddAnalyticsContextServices(this WebApplicationBuilder builder)
    {
        //Repositories (Infrastructure Layer)
        builder.Services.AddScoped<IAnalyticsRepository, AnalyticsRepository>();
        
        //Domain/application Services
        builder.Services.AddScoped<IAnalyticsQueryService, AnalyticsQueryService>();
        builder.Services.AddScoped<IAnalyticsCommandService, AnalyticsCommandService>();
        
        //Infrastructure Services - Reports
        builder.Services.AddScoped<IAnalyticsReportBuilder, QuestPdfAnalyticsReportBuilder>();
    }
}