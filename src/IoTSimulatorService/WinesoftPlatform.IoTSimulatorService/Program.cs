using Microsoft.OpenApi.Models;
using WinesoftPlatform.IoTSimulatorService.Application.Engine;

var builder = WebApplication.CreateBuilder(args);

// CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

// Controllers
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "WineSoft IoT Simulator API",
        Version = "v1",
        Description = "IoT Device Simulation Engine for the WineSoft platform. " +
                      "Generates realistic telemetry data (80% normal, 20% anomalies) " +
                      "and pushes readings to the InventoryService.",
    });
    options.EnableAnnotations();
});

// HTTP client for sending telemetry to InventoryService
builder.Services.AddHttpClient("InventoryService");

// Register the simulation engine as a hosted background service
builder.Services.AddHostedService<SimulationEngine>();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.UseCors("AllowAll");

app.MapGet("/health", () => Results.Ok(new
{
    service = "IoTSimulatorService",
    status = "healthy",
    timestamp = DateTime.UtcNow
}));

app.MapControllers();
app.Run();
