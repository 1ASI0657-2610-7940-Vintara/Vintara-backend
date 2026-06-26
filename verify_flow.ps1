$ErrorActionPreference = "Stop"

# Use local port of Gateway (5000)
$baseUrl = "http://localhost:5000"

# Generate random user details
$rand = Get-Random -Minimum 1000 -Maximum 9999
$username = "user_$rand"
$email = "user_$rand@vintara.com"
$password = "SecretPassword123!"

Write-Host "1. Registering user '$username'..." -ForegroundColor Cyan
$regBody = @{
    username = $username
    email = $email
    password = $password
} | ConvertTo-Json

$regResponse = Invoke-RestMethod -Uri "$baseUrl/api/auth/register" -Method Post -Body $regBody -ContentType "application/json"
Write-Host "Registration response: $regResponse" -ForegroundColor Green

Write-Host "2. Logging in..." -ForegroundColor Cyan
$loginBody = @{
    username = $username
    password = $password
} | ConvertTo-Json

$loginResponse = Invoke-RestMethod -Uri "$baseUrl/api/auth/login" -Method Post -Body $loginBody -ContentType "application/json"
$token = $loginResponse.token
Write-Host "JWT Token acquired successfully." -ForegroundColor Green

$headers = @{
    "Authorization" = "Bearer $token"
}

# Clear any keys in redis just to be clean (optional, but keep it clean)
Write-Host "Cleaning Redis for testing..." -ForegroundColor Yellow
$null = docker exec winesoft_redis redis-cli FLUSHALL

Write-Host "3. Querying Analytics Supply Levels to trigger cache..." -ForegroundColor Cyan
$levelsResponse1 = Invoke-RestMethod -Uri "$baseUrl/api/analytics/supply-levels" -Method Get -Headers $headers
Write-Host "First Query Response: $levelsResponse1" -ForegroundColor Green

Write-Host "4. Querying Analytics Inventory KPIs to trigger cache..." -ForegroundColor Cyan
$kpisResponse1 = Invoke-RestMethod -Uri "$baseUrl/api/analytics/inventory-kpis" -Method Get -Headers $headers
Write-Host "Second Query Response: $kpisResponse1" -ForegroundColor Green

Write-Host "5. Verifying cache keys are stored in Redis..." -ForegroundColor Cyan
$redisKeys = docker exec winesoft_redis redis-cli KEYS "*"
Write-Host "Redis keys: $redisKeys" -ForegroundColor Green

$hasCacheKeys = $redisKeys -match "analytics:"
if ($hasCacheKeys) {
    Write-Host "SUCCESS: Cache keys found in Redis!" -ForegroundColor Green
} else {
    Write-Host "FAILURE: No cache keys found in Redis." -ForegroundColor Red
    exit 1
}

# 6. Create a new Supply (triggers stock changed event)
Write-Host "6. Creating a new Supply with stock to trigger cache invalidation..." -ForegroundColor Cyan
$supplyBody = @{
    supplyName = "Cabernet Grapes $rand"
    quantity = 100
    unit = "kg"
    supplier = "Mendoza Vintners"
    price = 12.50
    date = (Get-Date -Format "yyyy-MM-ddTHH:mm:ssZ")
} | ConvertTo-Json

$supplyResponse = Invoke-RestMethod -Uri "$baseUrl/api/inventory/supplies" -Method Post -Headers $headers -Body $supplyBody -ContentType "application/json"
$supplyId = $supplyResponse.id
Write-Host "Supply created with ID: $supplyId" -ForegroundColor Green

Write-Host "7. Waiting 4 seconds for RabbitMQ/MassTransit to process cache invalidation..." -ForegroundColor Cyan
Start-Sleep -Seconds 4

Write-Host "8. Verifying cache keys were deleted/invalidated in Redis..." -ForegroundColor Cyan
$redisKeysAfter = docker exec winesoft_redis redis-cli KEYS "*"
Write-Host "Redis keys after invalidation: $redisKeysAfter" -ForegroundColor Green

$hasCacheKeysAfter = $redisKeysAfter -match "analytics:"
if (-not $hasCacheKeysAfter) {
    Write-Host "SUCCESS: Cache keys successfully invalidated/removed from Redis!" -ForegroundColor Green
} else {
    Write-Host "FAILURE: Cache keys still exist in Redis after stock change." -ForegroundColor Red
    exit 1
}

# 9. Test order creation to invalidate again
Write-Host "9. Re-populating cache by querying KPIs..." -ForegroundColor Cyan
$levelsResponse2 = Invoke-RestMethod -Uri "$baseUrl/api/analytics/supply-levels" -Method Get -Headers $headers
$redisKeysRecreated = docker exec winesoft_redis redis-cli KEYS "*"
Write-Host "Redis keys populated again: $redisKeysRecreated" -ForegroundColor Green

# Create Order with a specific Correlation ID
$correlationId = "corr-test-$rand-" + (New-Guid).ToString()
$headersWithCorr = @{
    "Authorization" = "Bearer $token"
    "X-Correlation-Id" = $correlationId
}

Write-Host "10. Creating a Purchase Order to trigger OrderCreated invalidation..." -ForegroundColor Cyan
$orderBody = @{
    productId = $supplyId
    supplier = "Mendoza Vintners"
    quantity = 15
    status = "Pending"
} | ConvertTo-Json

$orderResponse = Invoke-RestMethod -Uri "$baseUrl/api/purchases" -Method Post -Headers $headersWithCorr -Body $orderBody -ContentType "application/json"
Write-Host "Order created successfully. OrderId: $($orderResponse.id)" -ForegroundColor Green

Write-Host "11. Waiting 4 seconds for RabbitMQ/MassTransit OrderCreated consumer to invalidate cache..." -ForegroundColor Cyan
Start-Sleep -Seconds 4

Write-Host "12. Checking Redis keys again..." -ForegroundColor Cyan
$redisKeysFinal = docker exec winesoft_redis redis-cli KEYS "*"
Write-Host "Redis keys after order invalidation: $redisKeysFinal" -ForegroundColor Green

$hasCacheKeysFinal = $redisKeysFinal -match "analytics:"
if (-not $hasCacheKeysFinal) {
    Write-Host "VERIFICATION PASSED: Redis caching, distributed key tracking, and event-driven cache invalidation are fully functional!" -ForegroundColor Green
    exit 0
} else {
    Write-Host "VERIFICATION FAILED: Cache keys were not invalidated on OrderCreated." -ForegroundColor Red
    exit 1
}
