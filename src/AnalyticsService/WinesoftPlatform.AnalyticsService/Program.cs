using Microsoft.OpenApi.Models;
using WinesoftPlatform.API.Analytics.Application.Internal.QueryServices;
using WinesoftPlatform.API.Analytics.Application.Internal.CommandServices;
using WinesoftPlatform.API.Analytics.Domain.Repositories;
using WinesoftPlatform.API.Analytics.Domain.Services;
using WinesoftPlatform.API.Analytics.Infrastructure.Persistence.Repositories;
using WinesoftPlatform.API.Analytics.Infrastructure.Services;
using WinesoftPlatform.API.Shared.Infrastructure.Interfaces.ASAP.Configuration;
using WinesoftPlatform.AnalyticsService.Infrastructure.ExternalServices;
using Serilog;
using WinesoftPlatform.API.Shared.Infrastructure.Middleware;
using MassTransit;
using WinesoftPlatform.AnalyticsService.Application.Internal.Consumers;
using WinesoftPlatform.AnalyticsService.Infrastructure.Services;

var builder = WebApplication.CreateBuilder(args);

// Configure Serilog
builder.Host.UseSerilog((context, configuration) =>
{
    configuration
        .Enrich.FromLogContext()
        .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] ({CorrelationId}) {Message:lj}{NewLine}{Exception}")
        .WriteTo.File("logs/analytics-log-.txt", rollingInterval: RollingInterval.Day, outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] ({CorrelationId}) {Message:lj}{NewLine}{Exception}");
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

// Token Validation
var jwtKey = builder.Configuration["Jwt:Key"];
var jwtIssuer = builder.Configuration["Jwt:Issuer"];
var jwtAudience = builder.Configuration["Jwt:Audience"];

if (string.IsNullOrEmpty(jwtKey))
{
    throw new InvalidOperationException("JWT Key is not configured.");
}
if (string.IsNullOrEmpty(jwtIssuer))
{
    throw new InvalidOperationException("JWT Issuer is not configured.");
}
if (string.IsNullOrEmpty(jwtAudience))
{
    throw new InvalidOperationException("JWT Audience is not configured.");
}

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtIssuer,
        ValidAudience = jwtAudience,
        IssuerSigningKey = new Microsoft.IdentityModel.Tokens.SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(jwtKey))
    };
});

builder.Services.AddHttpContextAccessor();

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

// Redis Cache Configuration
builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = builder.Configuration["Redis:Configuration"] ?? "localhost:6379";
});

// Configure MassTransit with RabbitMQ and Consumers
builder.Services.AddMassTransit(x =>
{
    x.AddConsumer<OrderCreatedConsumer>();
    x.AddConsumer<SupplyStockChangedConsumer>();

    x.UsingRabbitMq((context, cfg) =>
    {
        var rabbitHost = builder.Configuration["RabbitMq:Host"] ?? "localhost";
        cfg.Host(rabbitHost, "/", h =>
        {
            h.Username("guest");
            h.Password("guest");
        });

        cfg.ReceiveEndpoint("analytics-order-created-queue", e =>
        {
            e.ConfigureConsumer<OrderCreatedConsumer>(context);
        });

        cfg.ReceiveEndpoint("analytics-stock-changed-queue", e =>
        {
            e.ConfigureConsumer<SupplyStockChangedConsumer>(context);
        });
    });
});

// Dependency Injection
builder.Services.AddScoped<IAnalyticsCacheService, AnalyticsCacheService>();
builder.Services.AddScoped<IAnalyticsRepository, AnalyticsRepository>();
builder.Services.AddScoped<IAnalyticsQueryService, AnalyticsQueryService>();
builder.Services.AddScoped<IAnalyticsCommandService, AnalyticsCommandService>();
builder.Services.AddScoped<IAnalyticsReportBuilder, QuestPdfAnalyticsReportBuilder>();

builder.Services.AddHealthChecks();

var app = builder.Build();

// Correlation ID Middleware
app.UseMiddleware<CorrelationIdMiddleware>();

app.MapOpenApi();
app.UseSwagger();
app.UseSwaggerUI();


app.UseHttpsRedirection();
app.MapHealthChecks("/health");

var supportedCultures = new[] { "en", "es" };
var localizationOptions = new RequestLocalizationOptions()
    .SetDefaultCulture(supportedCultures[0])
    .AddSupportedCultures(supportedCultures)
    .AddSupportedUICultures(supportedCultures);

app.UseRequestLocalization(localizationOptions);

app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.Run();
