# Test Transaction Purchase Flow
$uri = "http://localhost:5000/api/transaction"
$headers = @{
    "Content-Type" = "application/json"
}

# Test 1: Purchase of $1200 (should award 100 points)
$body1 = @{
    userId = 1
    amount = 1200.00
    type = "purchase"
    date = "2024-07-20T10:00:00Z"
} | ConvertTo-Json

Write-Host "Testing purchase of $1200..." -ForegroundColor Green
try {
    $response1 = Invoke-WebRequest -Uri $uri -Method POST -Headers $headers -Body $body1
    Write-Host "Response Status: $($response1.StatusCode)" -ForegroundColor Green
    Write-Host "Response Body: $($response1.Content)" -ForegroundColor Yellow
} catch {
    Write-Host "Error: $($_.Exception.Message)" -ForegroundColor Red
    Write-Host "Response: $($_.Exception.Response)" -ForegroundColor Red
}

# Test 2: Purchase of $750 (should award 50 points)
$body2 = @{
    userId = 1
    amount = 750.00
    type = "purchase"
    date = "2024-07-20T10:00:00Z"
} | ConvertTo-Json

Write-Host "`nTesting purchase of $750..." -ForegroundColor Green
try {
    $response2 = Invoke-WebRequest -Uri $uri -Method POST -Headers $headers -Body $body2
    Write-Host "Response Status: $($response2.StatusCode)" -ForegroundColor Green
    Write-Host "Response Body: $($response2.Content)" -ForegroundColor Yellow
} catch {
    Write-Host "Error: $($_.Exception.Message)" -ForegroundColor Red
    Write-Host "Response: $($_.Exception.Response)" -ForegroundColor Red
}

# Test 3: Get user events
Write-Host "`nGetting user events..." -ForegroundColor Green
try {
    $eventsResponse = Invoke-WebRequest -Uri "http://localhost:5000/api/transaction/events?userId=1" -Method GET
    Write-Host "Events Response: $($eventsResponse.Content)" -ForegroundColor Yellow
} catch {
    Write-Host "Error getting events: $($_.Exception.Message)" -ForegroundColor Red
} 