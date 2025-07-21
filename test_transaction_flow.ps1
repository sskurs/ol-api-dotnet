# Comprehensive Transaction Flow Test
Write-Host "=== TRANSACTION FLOW TEST ===" -ForegroundColor Cyan

# Step 1: Check if API is accessible
Write-Host "`n1. Testing API connectivity..." -ForegroundColor Green
try {
    $healthResponse = Invoke-WebRequest -Uri "http://localhost:5000/api/transaction?userId=1&page=1&limit=1" -Method GET
    Write-Host "✅ API is accessible (Status: $($healthResponse.StatusCode))" -ForegroundColor Green
} catch {
    Write-Host "❌ API is not accessible: $($_.Exception.Message)" -ForegroundColor Red
    exit
}

# Step 2: Create a high-value transaction ($1200 - should award 100 points)
Write-Host "`n2. Creating transaction of $1200..." -ForegroundColor Green
$transactionBody = @{
    userId = 1
    amount = 1200.00
    type = "purchase"
    date = "2024-07-20T10:00:00Z"
} | ConvertTo-Json

try {
    $response = Invoke-WebRequest -Uri "http://localhost:5000/api/transaction" -Method POST -Headers @{"Content-Type"="application/json"} -Body $transactionBody
    Write-Host "✅ Transaction created successfully!" -ForegroundColor Green
    Write-Host "Response Status: $($response.StatusCode)" -ForegroundColor Yellow
    Write-Host "Response Body: $($response.Content)" -ForegroundColor Yellow
    
    # Parse the response
    $result = $response.Content | ConvertFrom-Json
    Write-Host "`n📊 Transaction Details:" -ForegroundColor Cyan
    Write-Host "  - Transaction ID: $($result.transaction.id)" -ForegroundColor White
    Write-Host "  - Amount: $($result.transaction.amount)" -ForegroundColor White
    Write-Host "  - Points Awarded: $($result.pointsAwarded)" -ForegroundColor White
    Write-Host "  - Event ID: $($result.eventId)" -ForegroundColor White
    Write-Host "  - Event Name: $($result.eventName)" -ForegroundColor White
    Write-Host "  - Event Description: $($result.eventDescription)" -ForegroundColor White
    
} catch {
    Write-Host "❌ Transaction creation failed: $($_.Exception.Message)" -ForegroundColor Red
    if ($_.Exception.Response) {
        $errorResponse = $_.Exception.Response.GetResponseStream()
        $reader = New-Object System.IO.StreamReader($errorResponse)
        $errorContent = $reader.ReadToEnd()
        Write-Host "Error Details: $errorContent" -ForegroundColor Red
    }
}

# Step 3: Check if transaction was saved
Write-Host "`n3. Verifying transaction was saved..." -ForegroundColor Green
try {
    $transactionsResponse = Invoke-WebRequest -Uri "http://localhost:5000/api/transaction?userId=1&page=1&limit=5" -Method GET
    $transactions = $transactionsResponse.Content | ConvertFrom-Json
    Write-Host "✅ Found $($transactions.total) transactions for user" -ForegroundColor Green
    if ($transactions.transactions.Count -gt 0) {
        Write-Host "Latest transaction: $($transactions.transactions[0].amount)" -ForegroundColor Yellow
    }
} catch {
    Write-Host "❌ Failed to retrieve transactions: $($_.Exception.Message)" -ForegroundColor Red
}

# Step 4: Check custom events
Write-Host "`n4. Checking custom events..." -ForegroundColor Green
try {
    $eventsResponse = Invoke-WebRequest -Uri "http://localhost:5000/api/transaction/events?userId=1&page=1&limit=5" -Method GET
    $events = $eventsResponse.Content | ConvertFrom-Json
    Write-Host "✅ Found $($events.total) custom events for user" -ForegroundColor Green
    if ($events.events.Count -gt 0) {
        Write-Host "Latest event: $($events.events[0].eventName) - $($events.events[0].description)" -ForegroundColor Yellow
    }
} catch {
    Write-Host "❌ Failed to retrieve events: $($_.Exception.Message)" -ForegroundColor Red
}

# Step 5: Test another transaction ($750 - should award 50 points)
Write-Host "`n5. Creating transaction of $750..." -ForegroundColor Green
$transactionBody2 = @{
    userId = 1
    amount = 750.00
    type = "purchase"
    date = "2024-07-20T10:00:00Z"
} | ConvertTo-Json

try {
    $response2 = Invoke-WebRequest -Uri "http://localhost:5000/api/transaction" -Method POST -Headers @{"Content-Type"="application/json"} -Body $transactionBody2
    Write-Host "✅ Second transaction created successfully!" -ForegroundColor Green
    $result2 = $response2.Content | ConvertFrom-Json
    Write-Host "Points Awarded: $($result2.pointsAwarded)" -ForegroundColor Yellow
} catch {
    Write-Host "❌ Second transaction failed: $($_.Exception.Message)" -ForegroundColor Red
}

Write-Host "`n=== TEST COMPLETE ===" -ForegroundColor Cyan 