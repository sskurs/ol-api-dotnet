Write-Host "=== COMPREHENSIVE TRANSACTION FLOW TEST ===" -ForegroundColor Cyan

# Test 1: Transaction with amount >= 1000 (should award 100 points)
Write-Host "`n1. Testing transaction >= 1000 for 100 points..." -ForegroundColor Green
try {
    $body = @{
        userId = 6
        amount = 1500
        type = "purchase"
    } | ConvertTo-Json

    $response = Invoke-WebRequest -Uri "http://localhost:5000/api/transaction" -Method POST -Headers @{"Content-Type"="application/json"} -Body $body
    $result = $response.Content | ConvertFrom-Json
    
    Write-Host "✅ Transaction created successfully!" -ForegroundColor Green
    Write-Host "📊 Transaction Summary:" -ForegroundColor Yellow
    Write-Host "  Transaction ID: $($result.transaction.id)" -ForegroundColor White
    Write-Host "  Amount: $($result.transaction.amount)" -ForegroundColor White
    Write-Host "  Points Awarded: $($result.pointsAwarded)" -ForegroundColor White
    Write-Host "  Event Name: $($result.eventName)" -ForegroundColor White
    Write-Host "  Event Description: $($result.eventDescription)" -ForegroundColor White
    
} catch {
    Write-Host "❌ Transaction failed: $($_.Exception.Message)" -ForegroundColor Red
}

# Test 2: Transaction with amount >= 500 but < 1000 (should award 50 points)
Write-Host "`n2. Testing transaction >= 500 for 50 points..." -ForegroundColor Green
try {
    $body = @{
        userId = 6
        amount = 750
        type = "purchase"
    } | ConvertTo-Json

    $response = Invoke-WebRequest -Uri "http://localhost:5000/api/transaction" -Method POST -Headers @{"Content-Type"="application/json"} -Body $body
    $result = $response.Content | ConvertFrom-Json
    
    Write-Host "✅ Transaction created successfully!" -ForegroundColor Green
    Write-Host "📊 Transaction Summary:" -ForegroundColor Yellow
    Write-Host "  Transaction ID: $($result.transaction.id)" -ForegroundColor White
    Write-Host "  Amount: $($result.transaction.amount)" -ForegroundColor White
    Write-Host "  Points Awarded: $($result.pointsAwarded)" -ForegroundColor White
    Write-Host "  Event Name: $($result.eventName)" -ForegroundColor White
    Write-Host "  Event Description: $($result.eventDescription)" -ForegroundColor White
    
} catch {
    Write-Host "❌ Transaction failed: $($_.Exception.Message)" -ForegroundColor Red
}

# Test 3: Transaction with amount >= 100 but < 500 (should award 25 points)
Write-Host "`n3. Testing transaction >= 100 for 25 points..." -ForegroundColor Green
try {
    $body = @{
        userId = 6
        amount = 250
        type = "purchase"
    } | ConvertTo-Json

    $response = Invoke-WebRequest -Uri "http://localhost:5000/api/transaction" -Method POST -Headers @{"Content-Type"="application/json"} -Body $body
    $result = $response.Content | ConvertFrom-Json
    
    Write-Host "✅ Transaction created successfully!" -ForegroundColor Green
    Write-Host "📊 Transaction Summary:" -ForegroundColor Yellow
    Write-Host "  Transaction ID: $($result.transaction.id)" -ForegroundColor White
    Write-Host "  Amount: $($result.transaction.amount)" -ForegroundColor White
    Write-Host "  Points Awarded: $($result.pointsAwarded)" -ForegroundColor White
    Write-Host "  Event Name: $($result.eventName)" -ForegroundColor White
    Write-Host "  Event Description: $($result.eventDescription)" -ForegroundColor White
    
} catch {
    Write-Host "❌ Transaction failed: $($_.Exception.Message)" -ForegroundColor Red
}

# Test 4: Transaction with amount less than 100 (should award 0 points)
Write-Host "`n4. Testing transaction less than 100 for 0 points..." -ForegroundColor Green
try {
    $body = @{
        userId = 6
        amount = 50
        type = "purchase"
    } | ConvertTo-Json

    $response = Invoke-WebRequest -Uri "http://localhost:5000/api/transaction" -Method POST -Headers @{"Content-Type"="application/json"} -Body $body
    $result = $response.Content | ConvertFrom-Json
    
    Write-Host "✅ Transaction created successfully!" -ForegroundColor Green
    Write-Host "📊 Transaction Summary:" -ForegroundColor Yellow
    Write-Host "  Transaction ID: $($result.transaction.id)" -ForegroundColor White
    Write-Host "  Amount: $($result.transaction.amount)" -ForegroundColor White
    Write-Host "  Points Awarded: $($result.pointsAwarded)" -ForegroundColor White
    Write-Host "  Event Name: $($result.eventName)" -ForegroundColor White
    Write-Host "  Event Description: $($result.eventDescription)" -ForegroundColor White
    
} catch {
    Write-Host "❌ Transaction failed: $($_.Exception.Message)" -ForegroundColor Red
}

# Test 5: Get user's transactions
Write-Host "`n5. Getting user's transactions..." -ForegroundColor Green
try {
    $response = Invoke-WebRequest -Uri "http://localhost:5000/api/transaction?userId=6" -Method GET
    $result = $response.Content | ConvertFrom-Json
    
    Write-Host "✅ Retrieved transactions successfully!" -ForegroundColor Green
    Write-Host "📊 User Transactions Summary:" -ForegroundColor Yellow
    Write-Host "  Total Transactions: $($result.total)" -ForegroundColor White
    Write-Host "  Transactions in response: $($result.transactions.Count)" -ForegroundColor White
    
    foreach ($transaction in $result.transactions) {
        Write-Host "    - ID: $($transaction.id), Amount: $($transaction.amount), Type: $($transaction.type), Date: $($transaction.date)" -ForegroundColor Gray
    }
    
} catch {
    Write-Host "❌ Failed to get transactions: $($_.Exception.Message)" -ForegroundColor Red
}

Write-Host "`n=== COMPREHENSIVE TEST COMPLETE ===" -ForegroundColor Cyan 