Write-Host "=== TESTING POINTS AWARDING ===" -ForegroundColor Cyan

# Test with user ID 6 and amount >= 1000 to trigger points
Write-Host "Testing transaction >= 1000 for points..." -ForegroundColor Green
try {
    $body = @{
        userId = 6
        amount = 1500
        type = "purchase"
    } | ConvertTo-Json

    $response = Invoke-WebRequest -Uri "http://localhost:5000/api/transaction" -Method POST -Headers @{"Content-Type"="application/json"} -Body $body
    Write-Host "✅ Transaction created successfully!" -ForegroundColor Green
    Write-Host "Response: $($response.Content)" -ForegroundColor White
    
    # Parse the response to check points awarded
    $result = $response.Content | ConvertFrom-Json
    Write-Host "`n📊 Transaction Summary:" -ForegroundColor Yellow
    Write-Host "  Transaction ID: $($result.transaction.id)" -ForegroundColor White
    Write-Host "  Amount: $($result.transaction.amount)" -ForegroundColor White
    Write-Host "  Points Awarded: $($result.pointsAwarded)" -ForegroundColor White
    Write-Host "  Event ID: $($result.eventId)" -ForegroundColor White
    Write-Host "  Event Name: $($result.eventName)" -ForegroundColor White
    Write-Host "  Event Description: $($result.eventDescription)" -ForegroundColor White
    
} catch {
    Write-Host "❌ Transaction failed: $($_.Exception.Message)" -ForegroundColor Red
    if ($_.Exception.Response) {
        $stream = $_.Exception.Response.GetResponseStream()
        $reader = New-Object System.IO.StreamReader($stream)
        $errorContent = $reader.ReadToEnd()
        Write-Host "Error Details: $errorContent" -ForegroundColor Red
    }
}

Write-Host "`n=== POINTS TEST COMPLETE ===" -ForegroundColor Cyan 