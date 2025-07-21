Write-Host "=== TESTING USERS ===" -ForegroundColor Cyan

# Test 1: Check if there's a users endpoint
Write-Host "1. Testing users endpoint..." -ForegroundColor Green
try {
    $response = Invoke-WebRequest -Uri "http://localhost:5000/api/users" -Method GET
    Write-Host "✅ Users endpoint exists" -ForegroundColor Green
    Write-Host "Response: $($response.Content)" -ForegroundColor White
} catch {
    Write-Host "❌ Users endpoint not found: $($_.Exception.Message)" -ForegroundColor Red
}

# Test 2: Try to create a transaction with a known user ID
Write-Host "`n2. Testing with different user IDs..." -ForegroundColor Green

# Try user ID 1
Write-Host "Testing with user ID 1..." -ForegroundColor Yellow
try {
    $body = @{
        userId = 1
        amount = 1200
        type = "purchase"
    } | ConvertTo-Json

    $response = Invoke-WebRequest -Uri "http://localhost:5000/api/transaction" -Method POST -Headers @{"Content-Type"="application/json"} -Body $body
    Write-Host "✅ User ID 1 works!" -ForegroundColor Green
    Write-Host "Response: $($response.Content)" -ForegroundColor White
} catch {
    Write-Host "❌ User ID 1 failed: $($_.Exception.Message)" -ForegroundColor Red
    if ($_.Exception.Response) {
        $stream = $_.Exception.Response.GetResponseStream()
        $reader = New-Object System.IO.StreamReader($stream)
        $errorContent = $reader.ReadToEnd()
        Write-Host "Error Details: $errorContent" -ForegroundColor Red
    }
}

# Try user ID 2
Write-Host "`nTesting with user ID 2..." -ForegroundColor Yellow
try {
    $body = @{
        userId = 2
        amount = 1200
        type = "purchase"
    } | ConvertTo-Json

    $response = Invoke-WebRequest -Uri "http://localhost:5000/api/transaction" -Method POST -Headers @{"Content-Type"="application/json"} -Body $body
    Write-Host "✅ User ID 2 works!" -ForegroundColor Green
    Write-Host "Response: $($response.Content)" -ForegroundColor White
} catch {
    Write-Host "❌ User ID 2 failed: $($_.Exception.Message)" -ForegroundColor Red
    if ($_.Exception.Response) {
        $stream = $_.Exception.Response.GetResponseStream()
        $reader = New-Object System.IO.StreamReader($stream)
        $errorContent = $reader.ReadToEnd()
        Write-Host "Error Details: $errorContent" -ForegroundColor Red
    }
}

# Try user ID 3
Write-Host "`nTesting with user ID 3..." -ForegroundColor Yellow
try {
    $body = @{
        userId = 3
        amount = 1200
        type = "purchase"
    } | ConvertTo-Json

    $response = Invoke-WebRequest -Uri "http://localhost:5000/api/transaction" -Method POST -Headers @{"Content-Type"="application/json"} -Body $body
    Write-Host "✅ User ID 3 works!" -ForegroundColor Green
    Write-Host "Response: $($response.Content)" -ForegroundColor White
} catch {
    Write-Host "❌ User ID 3 failed: $($_.Exception.Message)" -ForegroundColor Red
    if ($_.Exception.Response) {
        $stream = $_.Exception.Response.GetResponseStream()
        $reader = New-Object System.IO.StreamReader($stream)
        $errorContent = $reader.ReadToEnd()
        Write-Host "Error Details: $errorContent" -ForegroundColor Red
    }
}

Write-Host "`n=== USER TEST COMPLETE ===" -ForegroundColor Cyan 