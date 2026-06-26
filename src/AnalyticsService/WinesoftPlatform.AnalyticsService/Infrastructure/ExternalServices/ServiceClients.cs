using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json.Serialization;

namespace WinesoftPlatform.AnalyticsService.Infrastructure.ExternalServices;

public class OrderDto
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public string SupplyName { get; set; } = string.Empty;
    public string Supplier { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public int OwnerId { get; set; }
}

public class SupplyDto
{
    public int Id { get; set; }
    public string SupplyName { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public string Unit { get; set; } = string.Empty;
    public string Supplier { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public DateTime Date { get; set; }
    public int OwnerId { get; set; }
}

public interface IInventoryServiceClient
{
    Task<IEnumerable<SupplyDto>> GetAllSuppliesAsync();
    Task<SupplyDto?> GetSupplyByIdAsync(int id);
}

public interface IPurchaseServiceClient
{
    Task<IEnumerable<OrderDto>> GetAllOrdersAsync();
}

public class InventoryServiceClient : IInventoryServiceClient
{
    private readonly HttpClient _httpClient;
    private readonly Microsoft.AspNetCore.Http.IHttpContextAccessor _httpContextAccessor;

    public InventoryServiceClient(HttpClient httpClient, Microsoft.AspNetCore.Http.IHttpContextAccessor httpContextAccessor)
    {
        _httpClient = httpClient;
        _httpContextAccessor = httpContextAccessor;
    }

    private void AddAuthHeader()
    {
        var context = _httpContextAccessor.HttpContext;
        if (context != null && context.Request.Headers.TryGetValue("Authorization", out var authHeader))
        {
            _httpClient.DefaultRequestHeaders.Authorization = AuthenticationHeaderValue.Parse(authHeader.ToString());
        }
    }

    public async Task<IEnumerable<SupplyDto>> GetAllSuppliesAsync()
    {
        AddAuthHeader();
        var response = await _httpClient.GetAsync("/api/v1/inventory/supplies");
        if (!response.IsSuccessStatusCode) return new List<SupplyDto>();
        return await response.Content.ReadFromJsonAsync<IEnumerable<SupplyDto>>() ?? new List<SupplyDto>();
    }

    public async Task<SupplyDto?> GetSupplyByIdAsync(int id)
    {
        AddAuthHeader();
        var response = await _httpClient.GetAsync($"/api/v1/inventory/supplies/{id}");
        if (!response.IsSuccessStatusCode) return null;
        return await response.Content.ReadFromJsonAsync<SupplyDto>();
    }
}

public class PurchaseServiceClient : IPurchaseServiceClient
{
    private readonly HttpClient _httpClient;
    private readonly Microsoft.AspNetCore.Http.IHttpContextAccessor _httpContextAccessor;

    public PurchaseServiceClient(HttpClient httpClient, Microsoft.AspNetCore.Http.IHttpContextAccessor httpContextAccessor)
    {
        _httpClient = httpClient;
        _httpContextAccessor = httpContextAccessor;
    }

    private void AddAuthHeader()
    {
        var context = _httpContextAccessor.HttpContext;
        if (context != null && context.Request.Headers.TryGetValue("Authorization", out var authHeader))
        {
            _httpClient.DefaultRequestHeaders.Authorization = AuthenticationHeaderValue.Parse(authHeader.ToString());
        }
    }

    public async Task<IEnumerable<OrderDto>> GetAllOrdersAsync()
    {
        AddAuthHeader();
        var response = await _httpClient.GetAsync("/api/v1/purchase-orders");
        if (!response.IsSuccessStatusCode) return new List<OrderDto>();
        return await response.Content.ReadFromJsonAsync<IEnumerable<OrderDto>>() ?? new List<OrderDto>();
    }
}
