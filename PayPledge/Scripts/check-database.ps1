# PayPledge Database Health Check Script
# Run this script to verify your Couchbase setup is working correctly

param(
    [string]$CouchbaseUrl = "http://localhost:8091",
    [string]$Username = "Administrator",
    [string]$Password = "password",
    [string]$BucketName = "paypledge"
)

Write-Host "PayPledge Database Health Check" -ForegroundColor Green
Write-Host "===============================" -ForegroundColor Green

$healthStatus = @{
    ServiceRunning        = $false
    WebConsoleAccessible  = $false
    AuthenticationWorking = $false
    BucketExists          = $false
    IndexesExist          = $false
    SampleDataExists      = $false
    ApplicationCanConnect = $false
}

function Test-CouchbaseService {
    Write-Host "Checking Couchbase service..." -ForegroundColor Cyan
    try {
        $service = Get-Service -Name "CouchbaseServer" -ErrorAction Stop
        if ($service.Status -eq "Running") {
            Write-Host "✅ Couchbase service is running" -ForegroundColor Green
            return $true
        }
        else {
            Write-Host "❌ Couchbase service is not running (Status: $($service.Status))" -ForegroundColor Red
            Write-Host "   Try: net start CouchbaseServer" -ForegroundColor Yellow
            return $false
        }
    }
    catch {
        Write-Host "❌ Couchbase service not found - is Couchbase installed?" -ForegroundColor Red
        return $false
    }
}

function Test-WebConsole {
    Write-Host "Checking web console accessibility..." -ForegroundColor Cyan
    try {
        $response = Invoke-WebRequest -Uri $CouchbaseUrl -TimeoutSec 10 -ErrorAction Stop
        if ($response.StatusCode -eq 200) {
            Write-Host "✅ Web console is accessible at $CouchbaseUrl" -ForegroundColor Green
            return $true
        }
    }
    catch {
        Write-Host "❌ Cannot access web console at $CouchbaseUrl" -ForegroundColor Red
        Write-Host "   Error: $($_.Exception.Message)" -ForegroundColor Yellow
        return $false
    }
}

function Test-Authentication {
    Write-Host "Testing authentication..." -ForegroundColor Cyan
    try {
        $credentials = [Convert]::ToBase64String([Text.Encoding]::ASCII.GetBytes("${Username}:${Password}"))
        $headers = @{
            "Authorization" = "Basic $credentials"
        }
        
        $response = Invoke-RestMethod -Uri "$CouchbaseUrl/pools/default" -Headers $headers -ErrorAction Stop
        Write-Host "✅ Authentication successful" -ForegroundColor Green
        return $true
    }
    catch {
        Write-Host "❌ Authentication failed" -ForegroundColor Red
        Write-Host "   Check username/password: $Username / $Password" -ForegroundColor Yellow
        return $false
    }
}

function Test-Bucket {
    Write-Host "Checking bucket existence..." -ForegroundColor Cyan
    try {
        $credentials = [Convert]::ToBase64String([Text.Encoding]::ASCII.GetBytes("${Username}:${Password}"))
        $headers = @{
            "Authorization" = "Basic $credentials"
        }
        
        $response = Invoke-RestMethod -Uri "$CouchbaseUrl/pools/default/buckets" -Headers $headers -ErrorAction Stop
        $bucket = $response | Where-Object { $_.name -eq $BucketName }
        
        if ($bucket) {
            Write-Host "✅ Bucket '$BucketName' exists" -ForegroundColor Green
            Write-Host "   Memory Quota: $($bucket.quota.ram / 1024 / 1024) MB" -ForegroundColor Gray
            Write-Host "   Document Count: $($bucket.basicStats.itemCount)" -ForegroundColor Gray
            return $true
        }
        else {
            Write-Host "❌ Bucket '$BucketName' not found" -ForegroundColor Red
            Write-Host "   Available buckets: $($response.name -join ', ')" -ForegroundColor Yellow
            return $false
        }
    }
    catch {
        Write-Host "❌ Failed to check buckets: $($_.Exception.Message)" -ForegroundColor Red
        return $false
    }
}

function Test-Indexes {
    Write-Host "Checking indexes..." -ForegroundColor Cyan
    try {
        $credentials = [Convert]::ToBase64String([Text.Encoding]::ASCII.GetBytes("${Username}:${Password}"))
        $headers = @{
            "Authorization" = "Basic $credentials"
            "Content-Type"  = "application/json"
        }
        
        $query = @{
            "statement" = "SELECT name, state FROM system:indexes WHERE keyspace_id = '$BucketName'"
        } | ConvertTo-Json
        
        $response = Invoke-RestMethod -Uri "$CouchbaseUrl/query/service" -Method POST -Body $query -Headers $headers -ErrorAction Stop
        
        if ($response.results -and $response.results.Count -gt 0) {
            Write-Host "✅ Found $($response.results.Count) indexes:" -ForegroundColor Green
            foreach ($index in $response.results) {
                $status = if ($index.state -eq "online") { "✅" } else { "⚠️" }
                Write-Host "   $status $($index.name) ($($index.state))" -ForegroundColor Gray
            }
            return $true
        }
        else {
            Write-Host "❌ No indexes found for bucket '$BucketName'" -ForegroundColor Red
            Write-Host "   Run the database-init.sql script to create indexes" -ForegroundColor Yellow
            return $false
        }
    }
    catch {
        Write-Host "❌ Failed to check indexes: $($_.Exception.Message)" -ForegroundColor Red
        return $false
    }
}

function Test-SampleData {
    Write-Host "Checking sample data..." -ForegroundColor Cyan
    try {
        $credentials = [Convert]::ToBase64String([Text.Encoding]::ASCII.GetBytes("${Username}:${Password}"))
        $headers = @{
            "Authorization" = "Basic $credentials"
            "Content-Type"  = "application/json"
        }
        
        $query = @{
            "statement" = "SELECT type, COUNT(*) as count FROM `$BucketName` WHERE type IS NOT NULL GROUP BY type ORDER BY type"
        } | ConvertTo-Json
        
        $response = Invoke-RestMethod -Uri "$CouchbaseUrl/query/service" -Method POST -Body $query -Headers $headers -ErrorAction Stop
        
        if ($response.results -and $response.results.Count -gt 0) {
            Write-Host "✅ Sample data found:" -ForegroundColor Green
            foreach ($result in $response.results) {
                Write-Host "   $($result.type): $($result.count) documents" -ForegroundColor Gray
            }
            return $true
        }
        else {
            Write-Host "⚠️ No sample data found" -ForegroundColor Yellow
            Write-Host "   This is normal for a fresh installation" -ForegroundColor Gray
            return $false
        }
    }
    catch {
        Write-Host "❌ Failed to check sample data: $($_.Exception.Message)" -ForegroundColor Red
        return $false
    }
}

function Test-ApplicationConnection {
    Write-Host "Testing application connection..." -ForegroundColor Cyan
    try {
        # Check if the application is running
        $appResponse = Invoke-WebRequest -Uri "https://localhost:5001" -TimeoutSec 5 -ErrorAction SilentlyContinue
        if ($appResponse -and $appResponse.StatusCode -eq 200) {
            Write-Host "✅ PayPledge application is running and accessible" -ForegroundColor Green
            return $true
        }
        else {
            Write-Host "⚠️ PayPledge application is not running" -ForegroundColor Yellow
            Write-Host "   Start it with: dotnet run" -ForegroundColor Gray
            return $false
        }
    }
    catch {
        Write-Host "⚠️ PayPledge application is not running" -ForegroundColor Yellow
        Write-Host "   Start it with: dotnet run" -ForegroundColor Gray
        return $false
    }
}

function Show-Summary {
    param($results)
    
    Write-Host ""
    Write-Host "=== HEALTH CHECK SUMMARY ===" -ForegroundColor Green
    Write-Host ""
    
    $totalChecks = $results.Keys.Count
    $passedChecks = ($results.Values | Where-Object { $_ -eq $true }).Count
    
    Write-Host "Overall Status: $passedChecks/$totalChecks checks passed" -ForegroundColor Cyan
    Write-Host ""
    
    foreach ($check in $results.Keys) {
        $status = if ($results[$check]) { "✅ PASS" } else { "❌ FAIL" }
        $color = if ($results[$check]) { "Green" } else { "Red" }
        Write-Host "$status $check" -ForegroundColor $color
    }
    
    Write-Host ""
    
    if ($passedChecks -eq $totalChecks) {
        Write-Host "🎉 All checks passed! Your database setup is working correctly." -ForegroundColor Green
    }
    elseif ($passedChecks -ge ($totalChecks - 2)) {
        Write-Host "⚠️ Most checks passed. Minor issues detected." -ForegroundColor Yellow
    }
    else {
        Write-Host "❌ Multiple issues detected. Please review the setup." -ForegroundColor Red
        Write-Host ""
        Write-Host "Recommended actions:" -ForegroundColor Cyan
        Write-Host "1. Check DATABASE_SETUP.md for detailed instructions" -ForegroundColor White
        Write-Host "2. Run setup-couchbase.ps1 script as Administrator" -ForegroundColor White
        Write-Host "3. Execute database-init.sql in Couchbase Query console" -ForegroundColor White
    }
}

# Run all health checks
Write-Host "Starting health checks..." -ForegroundColor Cyan
Write-Host ""

$healthStatus.ServiceRunning = Test-CouchbaseService
$healthStatus.WebConsoleAccessible = Test-WebConsole
$healthStatus.AuthenticationWorking = Test-Authentication
$healthStatus.BucketExists = Test-Bucket
$healthStatus.IndexesExist = Test-Indexes
$healthStatus.SampleDataExists = Test-SampleData
$healthStatus.ApplicationCanConnect = Test-ApplicationConnection

Show-Summary -results $healthStatus

Write-Host ""
Write-Host "Health check completed!" -ForegroundColor Green
