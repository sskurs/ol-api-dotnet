Write-Host "=== DEBUG TRANSACTION TEST ===" -ForegroundColor Cyan

# Test 1: Check if API is accessible
Write-Host "1. Testing API connectivity..." -ForegroundColor Green
try {
    $response = Invoke-WebRequest -Uri "http://localhost:5000/api/transaction/test" -Method GET
    Write-Host "✅ API is accessible" -ForegroundColor Green
} catch {
    Write-Host "❌ API is not accessible: $($_.Exception.Message)" -ForegroundColor Red
    exit
}

# Test 2: Try different transaction payloads
Write-Host "`n2. Testing different transaction payloads..." -ForegroundColor Green

# Test with minimal payload
Write-Host "Testing minimal payload..." -ForegroundColor Yellow
try {
    $minimalBody = @{
        userId = 1
        amount = 1200
        type = "purchase"
    } | ConvertTo-Json

    $response = Invoke-WebRequest -Uri "http://localhost:5000/api/transaction" -Method POST -Headers @{"Content-Type"="application/json"} -Body $minimalBody
    Write-Host "✅ Minimal payload worked!" -ForegroundColor Green
    Write-Host "Response: $($response.Content)" -ForegroundColor White
} catch {
    Write-Host "❌ Minimal payload failed: $($_.Exception.Message)" -ForegroundColor Red
    if ($_.Exception.Response) {
        $stream = $_.Exception.Response.GetResponseStream()
        $reader = New-Object System.IO.StreamReader($stream)
        $errorContent = $reader.ReadToEnd()
        Write-Host "Error Details: $errorContent" -ForegroundColor Red
    }
}

# Test with full payload
Write-Host "`nTesting full payload..." -ForegroundColor Yellow
try {
    $fullBody = @{
        userId = 1
        amount = 1200.00
        type = "purchase"
        date = "2024-07-20T10:00:00Z"
    } | ConvertTo-Json

    $response = Invoke-WebRequest -Uri "http://localhost:5000/api/transaction" -Method POST -Headers @{"Content-Type"="application/json"} -Body $fullBody
    Write-Host "✅ Full payload worked!" -ForegroundColor Green
    Write-Host "Response: $($response.Content)" -ForegroundColor White
} catch {
    Write-Host "❌ Full payload failed: $($_.Exception.Message)" -ForegroundColor Red
    if ($_.Exception.Response) {
        $stream = $_.Exception.Response.GetResponseStream()
        $reader = New-Object System.IO.StreamReader($stream)
        $errorContent = $reader.ReadToEnd()
        Write-Host "Error Details: $errorContent" -ForegroundColor Red
    }
}

Write-Host "`n=== DEBUG TEST COMPLETE ===" -ForegroundColor Cyan 