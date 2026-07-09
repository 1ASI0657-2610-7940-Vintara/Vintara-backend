using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using WinesoftPlatform.IoTSimulatorService.Application.Simulators;
using WinesoftPlatform.IoTSimulatorService.Domain.Interfaces;

namespace WinesoftPlatform.IoTSimulatorService.Application.Engine;

public record SupplyDto(
    int Id,
    string SupplyName,
    int Quantity,
    string Unit,
    int OwnerId
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
    private readonly Dictionary<string, int> _supplyOwnerMap = new();
    private readonly Dictionary<string, int> _supplyQuantityMap = new();
    private readonly HttpClient _httpClient;
    private readonly string _baseUrl;
    private readonly string _targetEndpoint;
    private readonly ILogger<SimulationEngine> _logger;
    private readonly int _intervalMs;

    private readonly string _authServiceUrl;
    private readonly string _clientId;
    private readonly string _clientSecret;
    private string? _serviceToken;

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

        _authServiceUrl = config["Simulation:AuthServiceUrl"] ?? "http://auth-service:8080";
        _clientId = config["Simulation:ClientId"] ?? "iot-simulator";
        _clientSecret = config["Simulation:ClientSecret"] ?? "iot-simulator-secret-key-123456";

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

    private async Task EnsureAuthenticatedAsync(CancellationToken ct)
    {
        if (string.IsNullOrEmpty(_serviceToken))
        {
            _serviceToken = await FetchServiceTokenAsync(ct);
        }
    }

    private async Task<string?> FetchServiceTokenAsync(CancellationToken ct)
    {
        try
        {
            var url = $"{_authServiceUrl.TrimEnd('/')}/api/v1/auth/service-token";
            var payload = new { ClientId = _clientId, ClientSecret = _clientSecret };
            var content = new StringContent(JsonSerializer.Serialize(payload, JsonOptions), Encoding.UTF8, "application/json");

            _logger.LogInformation("Requesting service token from {Url} with ClientId: {ClientId}", url, _clientId);
            var response = await _httpClient.PostAsync(url, content, ct);
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync(ct);
                using var doc = JsonDocument.Parse(json);
                if (doc.RootElement.TryGetProperty("token", out var tokenProp))
                {
                    var token = tokenProp.GetString();
                    _logger.LogInformation("Successfully obtained service token.");
                    return token;
                }
            }

            var errBody = await response.Content.ReadAsStringAsync(ct);
            _logger.LogError("Failed to obtain service token. Status: {StatusCode}, Error: {Error}", response.StatusCode, errBody);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception while fetching service token from {Url}", _authServiceUrl);
        }
        return null;
    }

    private async Task<List<SupplyDto>?> FetchActiveSuppliesAsync(CancellationToken ct)
    {
        try
        {
            await EnsureAuthenticatedAsync(ct);

            var url = $"{_baseUrl.TrimEnd('/')}/api/internal/supplies/all";
            var request = new HttpRequestMessage(HttpMethod.Get, url);
            if (!string.IsNullOrEmpty(_serviceToken))
            {
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _serviceToken);
            }

            var response = await _httpClient.SendAsync(request, ct);

            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                _logger.LogWarning("Unauthorized calling FetchActiveSuppliesAsync. Token might have expired. Refreshing token...");
                _serviceToken = null;
                await EnsureAuthenticatedAsync(ct);

                // Retry once
                request = new HttpRequestMessage(HttpMethod.Get, url);
                if (!string.IsNullOrEmpty(_serviceToken))
                {
                    request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _serviceToken);
                }
                response = await _httpClient.SendAsync(request, ct);
            }

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
            _supplyOwnerMap.Remove(key);
            _supplyQuantityMap.Remove(key);
            _logger.LogInformation("Removed IoT simulators for deleted supply: {Key}", key);
        }

        // 2. Add/update simulators for supplies
        foreach (var supply in supplies)
        {
            var supplyKey = $"SUPPLY-{supply.Id}";
            _supplyOwnerMap[supplyKey] = supply.OwnerId;
            _supplyQuantityMap[supplyKey] = supply.Quantity;

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

        foreach (var kvp in _activeSimulators)
        {
            var supplyKey = kvp.Key;
            var deviceList = kvp.Value;
            var ownerId = _supplyOwnerMap.TryGetValue(supplyKey, out var oid) ? oid : 0;
            var quantity = _supplyQuantityMap.TryGetValue(supplyKey, out var qty) ? qty : 0;

            foreach (var device in deviceList)
            {
                if (ct.IsCancellationRequested) return;

                try
                {
                    var reading = device.GenerateReading(quantity, ownerId);
                    await SendTelemetryAsync(reading, ct);

                    var logLevel = reading.IsAnomaly ? LogLevel.Warning : LogLevel.Debug;
                    _logger.Log(logLevel,
                        "[{DeviceId}] {SensorType} = {Value} {Unit} | Status: {Status} | OwnerId: {OwnerId}",
                        reading.DeviceId, reading.SensorType, reading.Value, reading.Unit, reading.Status, reading.OwnerId);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to send telemetry for device {DeviceId}", device.DeviceId);
                }
            }
        }
    }

    private async Task SendTelemetryAsync(Domain.Model.SensorReading reading, CancellationToken ct)
    {
        await EnsureAuthenticatedAsync(ct);

        var json = JsonSerializer.Serialize(reading, JsonOptions);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var request = new HttpRequestMessage(HttpMethod.Post, _targetEndpoint)
        {
            Content = content
        };
        if (!string.IsNullOrEmpty(_serviceToken))
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _serviceToken);
        }

        var response = await _httpClient.SendAsync(request, ct);

        if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
        {
            _logger.LogWarning("Unauthorized calling SendTelemetryAsync. Token might have expired. Refreshing token...");
            _serviceToken = null;
            await EnsureAuthenticatedAsync(ct);

            // Retry once
            request = new HttpRequestMessage(HttpMethod.Post, _targetEndpoint)
            {
                Content = new StringContent(json, Encoding.UTF8, "application/json")
            };
            if (!string.IsNullOrEmpty(_serviceToken))
            {
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _serviceToken);
            }
            response = await _httpClient.SendAsync(request, ct);
        }

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogWarning("InventoryService returned {StatusCode} for device {DeviceId}",
                response.StatusCode, reading.DeviceId);
        }
    }
}
