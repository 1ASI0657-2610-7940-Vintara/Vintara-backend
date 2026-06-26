$ErrorActionPreference = "Stop"

$baseUrl = "http://localhost:5000"
$inventoryUrl = "http://localhost:5002"
$authUrl = "http://localhost:5001"

# Generate random user details
$rand = Get-Random -Minimum 1000 -Maximum 9999
$username = "service_test_user_$rand"
$email = "service_test_user_$rand@vintara.com"
$password = "SecretPassword123!"

Write-Host "1. Registering a normal user '$username'..." -ForegroundColor Cyan
$regBody = @{
    username = $username
    email = $email
    password = $password
} | ConvertTo-Json

$regResponse = Invoke-RestMethod -Uri "$baseUrl/api/auth/register" -Method Post -Body $regBody -ContentType "application/json"
Write-Host "Registration response: $regResponse" -ForegroundColor Green

Write-Host "2. Logging in as normal user..." -ForegroundColor Cyan
$loginBody = @{
    username = $username
    password = $password
} | ConvertTo-Json

$loginResponse = Invoke-RestMethod -Uri "$baseUrl/api/auth/login" -Method Post -Body $loginBody -ContentType "application/json"
$userToken = $loginResponse.token
Write-Host "User JWT Token acquired." -ForegroundColor Green

$userHeaders = @{
    "Authorization" = "Bearer $userToken"
}

Write-Host "3. Verifying that a normal user token is rejected with 403 Forbidden when calling the internal endpoint directly..." -ForegroundColor Cyan
$directUrl = "$inventoryUrl/api/internal/supplies/all"
try {
    $res = Invoke-WebRequest -Uri $directUrl -Method Get -Headers $userHeaders -UseBasicParsing
    Write-Host "FAILURE: User token was accepted by internal endpoint! Status: $($res.StatusCode)" -ForegroundColor Red
    exit 1
} catch {
    $statusCode = $_.Exception.Response.StatusCode.value__
    if ($statusCode -eq 403) {
        Write-Host "SUCCESS: Direct call with user token returned 403 Forbidden (as expected)." -ForegroundColor Green
    } else {
        Write-Host "FAILURE: Unexpected status code from direct call: $statusCode" -ForegroundColor Red
        exit 1
    }
}

Write-Host "4. Verifying that calling through the Gateway returns 404 Not Found (since it is not exposed)..." -ForegroundColor Cyan
$gatewayUrl = "$baseUrl/api/inventory/internal/supplies/all"
try {
    $res = Invoke-WebRequest -Uri $gatewayUrl -Method Get -Headers $userHeaders -UseBasicParsing
    Write-Host "FAILURE: Gateway passed request and it succeeded!" -ForegroundColor Red
    exit 1
} catch {
    $statusCode = $_.Exception.Response.StatusCode.value__
    if ($statusCode -eq 404) {
        Write-Host "SUCCESS: Gateway call returned 404 Not Found (as expected)." -ForegroundColor Green
    } else {
        Write-Host "FAILURE: Unexpected status code from Gateway call: $statusCode" -ForegroundColor Red
        exit 1
    }
}

Write-Host "5. Requesting service token..." -ForegroundColor Cyan
$serviceTokenBody = @{
    clientId = "iot-simulator"
    clientSecret = "iot-simulator-secret-key-123456"
} | ConvertTo-Json

$serviceTokenResponse = Invoke-RestMethod -Uri "$authUrl/api/v1/auth/service-token" -Method Post -Body $serviceTokenBody -ContentType "application/json"
$serviceToken = $serviceTokenResponse.token
Write-Host "Service JWT Token acquired successfully." -ForegroundColor Green

$serviceHeaders = @{
    "Authorization" = "Bearer $serviceToken"
}

Write-Host "6. Verifying that the service token is accepted (200 OK)..." -ForegroundColor Cyan
try {
    $res = Invoke-RestMethod -Uri $directUrl -Method Get -Headers $serviceHeaders
    Write-Host "SUCCESS: Service token was accepted and returned $($res.Count) supplies." -ForegroundColor Green
} catch {
    Write-Host "FAILURE: Service token was rejected: $_" -ForegroundColor Red
    exit 1
}

Write-Host "`nVERIFICATION COMPLETED SUCCESSFULLY!" -ForegroundColor Green
exit 0
