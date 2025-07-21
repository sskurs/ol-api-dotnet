Write-Host "Testing Transaction API..." -ForegroundColor Green

# Test 1: Create a transaction
$body = @{
    userId = 1
    amount = 1200.00
    type = "purchase"
    date = "2024-07-20T10:00:00Z"
} | ConvertTo-Json

Write-Host "Creating transaction of $1200..." -ForegroundColor Yellow
try {
    $response = Invoke-WebRequest -Uri "http://localhost:5000/api/transaction" -Method POST -Headers @{"Content-Type"="application/json"} -Body $body
    Write-Host "Success! Status: $($response.StatusCode)" -ForegroundColor Green
    Write-Host "Response: $($response.Content)" -ForegroundColor White
} catch {
    Write-Host "Error: $($_.Exception.Message)" -ForegroundColor Red
}

# Test 2: Get transactions
Write-Host "`nGetting transactions..." -ForegroundColor Yellow
try {
    $transactions = Invoke-WebRequest -Uri "http://localhost:5000/api/transaction?userId=1&page=1&limit=5" -Method GET
    Write-Host "Success! Status: $($transactions.StatusCode)" -ForegroundColor Green
    Write-Host "Response: $($transactions.Content)" -ForegroundColor White
} catch {
    Write-Host "Error: $($_.Exception.Message)" -ForegroundColor Red
} 