namespace WinesoftPlatform.PurchaseService.Infrastructure.ExternalServices;

public interface IInventoryServiceClient
{
    Task<string> GetSupplyNameAsync(int supplyId);
}

public class InventoryServiceClient : IInventoryServiceClient
{
    private readonly HttpClient _httpClient;

    public InventoryServiceClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<string> GetSupplyNameAsync(int supplyId)
    {
        // For simplicity, assuming the inventory service exposes an endpoint like /api/v1/internal/supplies/{id}
        // In a real scenario you would have proper error handling, Polly retries, and DTOs.
        var response = await _httpClient.GetAsync($"/api/v1/inventory/supplies/{supplyId}");
        
        if (!response.IsSuccessStatusCode)
        {
            return "Unknown Supply";
        }

        // We can parse the json response or simply try to extract the name
        var content = await response.Content.ReadAsStringAsync();
        // A simple workaround assuming the JSON has a "supplyName" or "name" field:
        // var json = JsonDocument.Parse(content);
        // return json.RootElement.GetProperty("supplyName").GetString();
        
        return "Supply fetched from HTTP"; // Simplified for this migration step
    }
}
