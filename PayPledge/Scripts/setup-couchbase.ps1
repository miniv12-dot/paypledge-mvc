# PayPledge Couchbase Setup Script
# Run this script as Administrator to automate Couchbase installation and configuration

param(
    [switch]$SkipDownload,
    [switch]$SkipInstall,
    [switch]$ConfigureOnly,
    [string]$AdminPassword = "password"
)

Write-Host "PayPledge Couchbase Database Setup Script" -ForegroundColor Green
Write-Host "=========================================" -ForegroundColor Green

# Configuration variables
$CouchbaseVersion = "7.2.4"
$DownloadUrl = "https://packages.couchbase.com/releases/$CouchbaseVersion/couchbase-server-community_$CouchbaseVersion-windows_amd64.msi"
$InstallerPath = "$env:TEMP\couchbase-server-community.msi"
$CouchbaseUrl = "http://localhost:8091"
$BucketName = "paypledge"
$AdminUsername = "Administrator"

function Test-Administrator {
    $currentUser = [Security.Principal.WindowsIdentity]::GetCurrent()
    $principal = New-Object Security.Principal.WindowsPrincipal($currentUser)
    return $principal.IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)
}

function Wait-ForCouchbase {
    param([int]$TimeoutSeconds = 120)
    
    Write-Host "Waiting for Couchbase to start..." -ForegroundColor Yellow
    $timeout = (Get-Date).AddSeconds($TimeoutSeconds)
    
    do {
        try {
            $response = Invoke-WebRequest -Uri $CouchbaseUrl -TimeoutSec 5 -ErrorAction SilentlyContinue
            if ($response.StatusCode -eq 200) {
                Write-Host "Couchbase is running!" -ForegroundColor Green
                return $true
            }
        }
        catch {
            # Continue waiting
        }
        
        Start-Sleep -Seconds 5
        Write-Host "." -NoNewline -ForegroundColor Yellow
        
    } while ((Get-Date) -lt $timeout)
    
    Write-Host ""
    Write-Host "Timeout waiting for Couchbase to start" -ForegroundColor Red
    return $false
}

function Test-CouchbaseInstalled {
    $service = Get-Service -Name "CouchbaseServer" -ErrorAction SilentlyContinue
    return $service -ne $null
}

function Download-Couchbase {
    if ($SkipDownload) {
        Write-Host "Skipping download..." -ForegroundColor Yellow
        return
    }
    
    Write-Host "Downloading Couchbase Server Community Edition..." -ForegroundColor Cyan
    try {
        Invoke-WebRequest -Uri $DownloadUrl -OutFile $InstallerPath -UseBasicParsing
        Write-Host "Download completed: $InstallerPath" -ForegroundColor Green
    }
    catch {
        Write-Host "Failed to download Couchbase: $($_.Exception.Message)" -ForegroundColor Red
        exit 1
    }
}

function Install-Couchbase {
    if ($SkipInstall -or (Test-CouchbaseInstalled)) {
        Write-Host "Couchbase is already installed or installation skipped" -ForegroundColor Yellow
        return
    }
    
    Write-Host "Installing Couchbase Server..." -ForegroundColor Cyan
    try {
        $process = Start-Process -FilePath "msiexec.exe" -ArgumentList "/i", $InstallerPath, "/quiet", "/norestart" -Wait -PassThru
        if ($process.ExitCode -eq 0) {
            Write-Host "Couchbase installation completed successfully" -ForegroundColor Green
        }
        else {
            Write-Host "Couchbase installation failed with exit code: $($process.ExitCode)" -ForegroundColor Red
            exit 1
        }
    }
    catch {
        Write-Host "Failed to install Couchbase: $($_.Exception.Message)" -ForegroundColor Red
        exit 1
    }
}

function Start-CouchbaseService {
    Write-Host "Starting Couchbase Server service..." -ForegroundColor Cyan
    try {
        Start-Service -Name "CouchbaseServer" -ErrorAction Stop
        Write-Host "Couchbase service started successfully" -ForegroundColor Green
    }
    catch {
        Write-Host "Failed to start Couchbase service: $($_.Exception.Message)" -ForegroundColor Red
        Write-Host "You may need to start it manually from Services.msc" -ForegroundColor Yellow
    }
}

function Configure-CouchbaseCluster {
    Write-Host "Configuring Couchbase cluster..." -ForegroundColor Cyan
    
    # Wait for Couchbase to be ready
    if (-not (Wait-ForCouchbase)) {
        Write-Host "Cannot configure Couchbase - service not responding" -ForegroundColor Red
        return $false
    }
    
    try {
        # Setup cluster
        $setupBody = @{
            "clusterName"      = "PayPledge-Cluster"
            "services"         = @("kv", "n1ql", "index")
            "memoryQuota"      = 2048
            "indexMemoryQuota" = 512
        } | ConvertTo-Json
        
        $setupResponse = Invoke-RestMethod -Uri "$CouchbaseUrl/pools/default" -Method POST -Body $setupBody -ContentType "application/json" -ErrorAction SilentlyContinue
        
        # Setup admin credentials
        $credentialsBody = @{
            "username" = $AdminUsername
            "password" = $AdminPassword
            "port"     = "SAME"
        } | ConvertTo-Json
        
        $credResponse = Invoke-RestMethod -Uri "$CouchbaseUrl/settings/web" -Method POST -Body $credentialsBody -ContentType "application/json" -ErrorAction SilentlyContinue
        
        Write-Host "Cluster configuration completed" -ForegroundColor Green
        return $true
    }
    catch {
        Write-Host "Cluster may already be configured or configuration failed: $($_.Exception.Message)" -ForegroundColor Yellow
        return $true  # Continue anyway
    }
}

function Create-PayPledgeBucket {
    Write-Host "Creating PayPledge bucket..." -ForegroundColor Cyan
    
    try {
        $credentials = [Convert]::ToBase64String([Text.Encoding]::ASCII.GetBytes("${AdminUsername}:${AdminPassword}"))
        $headers = @{
            "Authorization" = "Basic $credentials"
            "Content-Type"  = "application/x-www-form-urlencoded"
        }
        
        $bucketBody = "name=$BucketName&bucketType=membase&ramQuotaMB=1024&replicaNumber=0&flushEnabled=1"
        
        $response = Invoke-RestMethod -Uri "$CouchbaseUrl/pools/default/buckets" -Method POST -Body $bucketBody -Headers $headers
        Write-Host "Bucket '$BucketName' created successfully" -ForegroundColor Green
        return $true
    }
    catch {
        if ($_.Exception.Response.StatusCode -eq 400) {
            Write-Host "Bucket '$BucketName' may already exist" -ForegroundColor Yellow
            return $true
        }
        Write-Host "Failed to create bucket: $($_.Exception.Message)" -ForegroundColor Red
        return $false
    }
}

function Show-NextSteps {
    Write-Host ""
    Write-Host "=== SETUP COMPLETE ===" -ForegroundColor Green
    Write-Host ""
    Write-Host "Next Steps:" -ForegroundColor Cyan
    Write-Host "1. Open Couchbase Web Console: $CouchbaseUrl" -ForegroundColor White
    Write-Host "2. Login with:" -ForegroundColor White
    Write-Host "   Username: $AdminUsername" -ForegroundColor White
    Write-Host "   Password: $AdminPassword" -ForegroundColor White
    Write-Host "3. Run the database initialization script:" -ForegroundColor White
    Write-Host "   - Go to Query tab in Couchbase Console" -ForegroundColor White
    Write-Host "   - Copy and paste contents from Scripts/database-init.sql" -ForegroundColor White
    Write-Host "4. Test your PayPledge application:" -ForegroundColor White
    Write-Host "   cd PayPledge" -ForegroundColor White
    Write-Host "   dotnet run" -ForegroundColor White
    Write-Host ""
    Write-Host "Configuration Details:" -ForegroundColor Cyan
    Write-Host "- Connection String: couchbase://localhost" -ForegroundColor White
    Write-Host "- Bucket Name: $BucketName" -ForegroundColor White
    Write-Host "- Admin Username: $AdminUsername" -ForegroundColor White
    Write-Host "- Admin Password: $AdminPassword" -ForegroundColor White
    Write-Host ""
}

# Main execution
try {
    Write-Host "Checking prerequisites..." -ForegroundColor Cyan
    
    if (-not (Test-Administrator)) {
        Write-Host "This script must be run as Administrator" -ForegroundColor Red
        Write-Host "Please right-click PowerShell and select 'Run as Administrator'" -ForegroundColor Yellow
        exit 1
    }
    
    if ($ConfigureOnly) {
        Write-Host "Configuration-only mode enabled" -ForegroundColor Yellow
    }
    else {
        # Download and install Couchbase
        Download-Couchbase
        Install-Couchbase
    }
    
    # Start service
    Start-CouchbaseService
    
    # Configure cluster and create bucket
    if (Configure-CouchbaseCluster) {
        Start-Sleep -Seconds 10  # Wait a bit more for cluster to be ready
        Create-PayPledgeBucket
    }
    
    Show-NextSteps
    
}
catch {
    Write-Host "Setup failed: $($_.Exception.Message)" -ForegroundColor Red
    Write-Host "Please check the DATABASE_SETUP.md file for manual installation steps" -ForegroundColor Yellow
    exit 1
}

Write-Host "Setup script completed!" -ForegroundColor Green
