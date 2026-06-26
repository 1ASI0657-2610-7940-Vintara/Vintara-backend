namespace WinesoftPlatform.PurchaseService.Infrastructure.ExternalServices;

public interface IInventoryServiceClient
{
    Task<string> GetSupplyNameAsync(int supplyId);
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

    public async Task<string> GetSupplyNameAsync(int supplyId)
    {
        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext != null && httpContext.Request.Headers.TryGetValue("Authorization", out var authHeader))
        {
            _httpClient.DefaultRequestHeaders.Authorization = 
                System.Net.Http.Headers.AuthenticationHeaderValue.Parse(authHeader.ToString());
        }

        if (httpContext != null && httpContext.Request.Headers.TryGetValue("X-Correlation-Id", out var correlationId))
        {
            _httpClient.DefaultRequestHeaders.Remove("X-Correlation-Id");
            _httpClient.DefaultRequestHeaders.Add("X-Correlation-Id", correlationId.ToString());
        }

        try
        {
            var response = await _httpClient.GetAsync($"/api/v1/inventory/supplies/{supplyId}");
            
            if (!response.IsSuccessStatusCode)
            {
                return $"Degraded: Supply name unavailable (HTTP {(int)response.StatusCode})";
            }

            return "Supply fetched from HTTP"; // Simplified for this migration step
        }
        catch (Exception ex)
        {
            return $"Degraded: Supply name unavailable ({ex.Message})";
        }
    }
}
