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

    public InventoryServiceClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<IEnumerable<SupplyDto>> GetAllSuppliesAsync()
    {
        var response = await _httpClient.GetAsync("/api/v1/inventory/supplies");
        if (!response.IsSuccessStatusCode) return new List<SupplyDto>();
        return await response.Content.ReadFromJsonAsync<IEnumerable<SupplyDto>>() ?? new List<SupplyDto>();
    }

    public async Task<SupplyDto?> GetSupplyByIdAsync(int id)
    {
        var response = await _httpClient.GetAsync($"/api/v1/inventory/supplies/{id}");
        if (!response.IsSuccessStatusCode) return null;
        return await response.Content.ReadFromJsonAsync<SupplyDto>();
    }
}

public class PurchaseServiceClient : IPurchaseServiceClient
{
    private readonly HttpClient _httpClient;

    public PurchaseServiceClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<IEnumerable<OrderDto>> GetAllOrdersAsync()
    {
        var response = await _httpClient.GetAsync("/api/v1/purchase-orders");
        if (!response.IsSuccessStatusCode) return new List<OrderDto>();
        return await response.Content.ReadFromJsonAsync<IEnumerable<OrderDto>>() ?? new List<OrderDto>();
    }
}
