using Microsoft.OpenApi.Models;
using WinesoftPlatform.API.Analytics.Application.Internal.QueryServices;
using WinesoftPlatform.API.Analytics.Domain.Repositories;
using WinesoftPlatform.API.Analytics.Domain.Services;
using WinesoftPlatform.API.Analytics.Infrastructure.Persistence.Repositories;
using WinesoftPlatform.API.Analytics.Infrastructure.Services;
using WinesoftPlatform.API.Shared.Infrastructure.Interfaces.ASAP.Configuration;
using WinesoftPlatform.AnalyticsService.Infrastructure.ExternalServices;

var builder = WebApplication.CreateBuilder(args);

// CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowLocalAndNetlify", policy =>
    {
        policy.WithOrigins(
                "https://winesoft-frontend.vercel.app",
                "https://winesoft-platform.onrender.com"
            )
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});

// Localization
builder.Services.AddLocalization(options => options.ResourcesPath = "Resources");

// Controllers
builder.Services.AddControllers(options =>
    options.Conventions.Add(new KebabCaseRouteNamingConvention()));

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "WineSoft Analytics API",
        Version = "v1",
        Description = "Analytics Service for the WineSoft platform.",
    });
    options.EnableAnnotations();
});
builder.Services.AddOpenApi();

// HTTP Clients
builder.Services.AddHttpClient<IInventoryServiceClient, InventoryServiceClient>(client =>
{
    var inventoryUrl = builder.Configuration.GetValue<string>("InventoryServiceUrl") ?? "http://localhost:5002";
    client.BaseAddress = new Uri(inventoryUrl);
});

builder.Services.AddHttpClient<IPurchaseServiceClient, PurchaseServiceClient>(client =>
{
    var purchaseUrl = builder.Configuration.GetValue<string>("PurchaseServiceUrl") ?? "http://localhost:5003";
    client.BaseAddress = new Uri(purchaseUrl);
});

// Dependency Injection
builder.Services.AddScoped<IAnalyticsRepository, AnalyticsRepository>();
builder.Services.AddScoped<IAnalyticsQueryService, AnalyticsQueryService>();
builder.Services.AddScoped<IAnalyticsReportBuilder, QuestPdfAnalyticsReportBuilder>();

var app = builder.Build();

app.MapOpenApi();
app.UseSwagger();
app.UseSwaggerUI();

app.UseCors("AllowLocalAndNetlify");
app.UseHttpsRedirection();

var supportedCultures = new[] { "en", "es" };
var localizationOptions = new RequestLocalizationOptions()
    .SetDefaultCulture(supportedCultures[0])
    .AddSupportedCultures(supportedCultures)
    .AddSupportedUICultures(supportedCultures);

app.UseRequestLocalization(localizationOptions);

app.UseAuthorization();
app.MapControllers();
app.Run();
