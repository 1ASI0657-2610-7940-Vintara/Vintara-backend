using System.Text;
using System.Text.Json;
using WinesoftPlatform.IoTSimulatorService.Application.Simulators;
using WinesoftPlatform.IoTSimulatorService.Domain.Interfaces;

namespace WinesoftPlatform.IoTSimulatorService.Application.Engine;

public record SupplyDto(
    int Id,
    string SupplyName,
    int Quantity,
    string Unit
);

/// <summary>
/// Dynamic simulation engine running as a BackgroundService.
/// Periodically queries the InventoryService to retrieve active supplies,
/// generates telemetry readings only for devices associated with those existing supplies,
/// and pushes them to the InventoryService's sensor-alerts endpoint via HTTP.
/// </summary>
public class SimulationEngine : BackgroundService
{
    private readonly Dictionary<string, List<IDeviceSimulator>> _activeSimulators = new();
    private readonly HttpClient _httpClient;
    private readonly string _baseUrl;
    private readonly string _targetEndpoint;
    private readonly ILogger<SimulationEngine> _logger;
    private readonly int _intervalMs;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false
    };

    public SimulationEngine(IHttpClientFactory httpClientFactory, IConfiguration config, ILogger<SimulationEngine> logger)
    {
        _httpClient = httpClientFactory.CreateClient("InventoryService");
        _logger = logger;
        _intervalMs = config.GetValue("Simulation:IntervalMs", 5000);

        _baseUrl = config["Simulation:InventoryServiceUrl"] ?? "http://inventory-service:8080";
        _targetEndpoint = $"{_baseUrl.TrimEnd('/')}/api/v1/inventory/sensor-alerts";

        _logger.LogInformation("SimulationEngine initialized. Target endpoint: {Endpoint}", _targetEndpoint);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("IoT Simulation Engine started. Interval: {Interval}ms", _intervalMs);

        // Give InventoryService time to start up and initialize DB
        await Task.Delay(15_000, stoppingToken);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var supplies = await FetchActiveSuppliesAsync(stoppingToken);
                if (supplies != null)
                {
                    UpdateSimulators(supplies);
                    await RunSimulationCycleAsync(stoppingToken);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred during simulation cycle.");
            }

            await Task.Delay(_intervalMs, stoppingToken);
        }

        _logger.LogInformation("IoT Simulation Engine stopped.");
    }

    private async Task<List<SupplyDto>?> FetchActiveSuppliesAsync(CancellationToken ct)
    {
        try
        {
            var url = $"{_baseUrl.TrimEnd('/')}/api/v1/inventory/supplies";
            var response = await _httpClient.GetAsync(url, ct);

            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync(ct);
                return JsonSerializer.Deserialize<List<SupplyDto>>(json, JsonOptions);
            }

            _logger.LogWarning("Failed to fetch supplies from InventoryService. Status: {StatusCode}", response.StatusCode);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to connect to InventoryService at {BaseUrl} to fetch supplies.", _baseUrl);
        }

        return null;
    }

    private void UpdateSimulators(List<SupplyDto> supplies)
    {
        var currentKeys = supplies.Select(s => $"SUPPLY-{s.Id}").ToHashSet();

        // 1. Remove simulators for deleted supplies
        var keysToRemove = _activeSimulators.Keys.Where(k => !currentKeys.Contains(k)).ToList();
        foreach (var key in keysToRemove)
        {
            _activeSimulators.Remove(key);
            _logger.LogInformation("Removed IoT simulators for deleted supply: {Key}", key);
        }

        // 2. Add simulators for newly created supplies
        foreach (var supply in supplies)
        {
            var supplyKey = $"SUPPLY-{supply.Id}";
            if (!_activeSimulators.ContainsKey(supplyKey))
            {
                var deviceList = new List<IDeviceSimulator>();
                var unitLower = supply.Unit.ToLowerInvariant();
                var nameLower = supply.SupplyName.ToLowerInvariant();

                // Classify supply to determine sensor types
                bool isLiquid = unitLower == "l" || unitLower == "litros" || unitLower == "liters" ||
                                nameLower.Contains("vino") || nameLower.Contains("pisco") || nameLower.Contains("mosto") || nameLower.Contains("jugo");

                if (isLiquid)
                {
                    // Liquid storage tank: simulate temperature, pressure, and level
                    var tankId = $"TANK-{supply.Id}";
                    deviceList.Add(DeviceSimulatorFactory.Create("temperature", tankId));
                    deviceList.Add(DeviceSimulatorFactory.Create("pressure", tankId));
                    deviceList.Add(DeviceSimulatorFactory.Create("level", tankId));
                    _logger.LogInformation("Registered TANK simulators for liquid supply '{Name}' (ID: {Id})", supply.SupplyName, supply.Id);
                }
                else
                {
                    // Dry/solid warehouse storage shelf: simulate level and humidity
                    var shelfId = $"SHELF-{supply.Id}";
                    deviceList.Add(DeviceSimulatorFactory.Create("level", shelfId));
                    deviceList.Add(DeviceSimulatorFactory.Create("humidity", shelfId));
                    _logger.LogInformation("Registered SHELF simulators for supply '{Name}' (ID: {Id})", supply.SupplyName, supply.Id);
                }

                _activeSimulators[supplyKey] = deviceList;
            }
        }
    }

    private async Task RunSimulationCycleAsync(CancellationToken ct)
    {
        if (_activeSimulators.Count == 0)
        {
            _logger.LogDebug("No active supplies found in inventory. Skipping telemetry generation.");
            return;
        }

        foreach (var deviceList in _activeSimulators.Values)
        {
            foreach (var device in deviceList)
            {
                if (ct.IsCancellationRequested) return;

                try
                {
                    var reading = device.GenerateReading();
                    await SendTelemetryAsync(reading);

                    var logLevel = reading.IsAnomaly ? LogLevel.Warning : LogLevel.Debug;
                    _logger.Log(logLevel,
                        "[{DeviceId}] {SensorType} = {Value} {Unit} | Status: {Status}",
                        reading.DeviceId, reading.SensorType, reading.Value, reading.Unit, reading.Status);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to send telemetry for device {DeviceId}", device.DeviceId);
                }
            }
        }
    }

    private async Task SendTelemetryAsync(Domain.Model.SensorReading reading)
    {
        var json = JsonSerializer.Serialize(reading, JsonOptions);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await _httpClient.PostAsync(_targetEndpoint, content);

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogWarning("InventoryService returned {StatusCode} for device {DeviceId}",
                response.StatusCode, reading.DeviceId);
        }
    }
}
