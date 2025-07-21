Write-Host "=== TESTING HIGHER USER IDs ===" -ForegroundColor Cyan

# Test with user IDs 1-10
for ($i = 1; $i -le 10; $i++) {
    Write-Host "Testing with user ID $i..." -ForegroundColor Yellow
    try {
        $body = @{
            userId = $i
            amount = 1200
            type = "purchase"
        } | ConvertTo-Json

        $response = Invoke-WebRequest -Uri "http://localhost:5000/api/transaction" -Method POST -Headers @{"Content-Type"="application/json"} -Body $body
        Write-Host "✅ User ID $i works!" -ForegroundColor Green
        Write-Host "Response: $($response.Content)" -ForegroundColor White
        break
    } catch {
        Write-Host "❌ User ID $i failed: $($_.Exception.Message)" -ForegroundColor Red
        if ($_.Exception.Response) {
            $stream = $_.Exception.Response.GetResponseStream()
            $reader = New-Object System.IO.StreamReader($stream)
            $errorContent = $reader.ReadToEnd()
            if ($errorContent -like "*User not found*") {
                Write-Host "  → User not found" -ForegroundColor Red
            } else {
                Write-Host "  → Error: $errorContent" -ForegroundColor Red
            }
        }
    }
}

Write-Host "`n=== HIGHER ID TEST COMPLETE ===" -ForegroundColor Cyan 