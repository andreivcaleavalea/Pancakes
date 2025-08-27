# Pancakes Production Data Seeding Script (Fixed Version)
param(
    [Parameter(Mandatory=$true)]
    [string]$ResourceGroup,
    
    [Parameter(Mandatory=$false)]
    [int]$Users = 100,
    
    [Parameter(Mandatory=$false)]
    [int]$Blogs = 1000,
    
    [Parameter(Mandatory=$false)]
    [bool]$ClearExisting = $true,
    
    [Parameter(Mandatory=$false)]
    [string]$AdminEmail = "admin@pancakes.com",
    
    [Parameter(Mandatory=$false)]
    [string]$AdminPassword = "AdminPassword123!"
)

Write-Host "🥞 Pancakes Production Data Seeding Script" -ForegroundColor Green
Write-Host "=========================================" -ForegroundColor Green

# Function to check if Azure CLI is logged in
function Test-AzureLogin {
    try {
        $account = az account show --output json 2>$null | ConvertFrom-Json
        if ($account) {
            Write-Host "✅ Logged in to Azure as: $($account.user.name)" -ForegroundColor Green
            return $true
        }
    }
    catch {
        Write-Host "❌ Not logged in to Azure. Please run 'az login'" -ForegroundColor Red
        return $false
    }
    return $false
}

# Function to get service URLs from Azure Container Apps
function Get-ServiceUrls {
    param([string]$ResourceGroup)
    
    Write-Host "🔍 Discovering service URLs from Azure Container Apps..." -ForegroundColor Yellow
    
    try {
        $apps = az containerapp list --resource-group $ResourceGroup --output json | ConvertFrom-Json
        
        $urls = @{}
        
        foreach ($app in $apps) {
            $fqdn = $app.properties.configuration.ingress.fqdn
            if ($fqdn) {
                $appName = $app.name
                $url = "https://$fqdn"
                
                switch -Wildcard ($appName) {
                    "*user*" { 
                        $urls["UserService"] = $url
                        Write-Host "  👤 UserService: $url" -ForegroundColor Cyan
                    }
                    "*blog*" { 
                        $urls["BlogService"] = $url
                        Write-Host "  📝 BlogService: $url" -ForegroundColor Cyan
                    }
                    "*admin*" { 
                        $urls["AdminService"] = $url
                        Write-Host "  🔐 AdminService: $url" -ForegroundColor Cyan
                    }
                    "*frontend*" { 
                        $urls["Frontend"] = $url
                        Write-Host "  🌐 Frontend: $url" -ForegroundColor Cyan
                    }
                }
            }
        }
        
        return $urls
    }
    catch {
        Write-Host "❌ Error discovering service URLs: $($_.Exception.Message)" -ForegroundColor Red
        throw
    }
}

# Function to get admin authentication token
function Get-AdminToken {
    param(
        [string]$AdminServiceUrl,
        [string]$Email,
        [string]$Password
    )
    
    Write-Host "🔑 Authenticating with admin service..." -ForegroundColor Yellow
    
    try {
        $loginBody = @{
            email = $Email
            password = $Password
        } | ConvertTo-Json
        
        $headers = @{
            "Content-Type" = "application/json"
        }
        
        $response = Invoke-RestMethod -Uri "$AdminServiceUrl/api/auth/login" -Method POST -Body $loginBody -Headers $headers
        
        if ($response.token) {
            Write-Host "✅ Successfully authenticated as admin" -ForegroundColor Green
            return $response.token
        } else {
            throw "No token received from admin service"
        }
    }
    catch {
        Write-Host "❌ Admin authentication failed: $($_.Exception.Message)" -ForegroundColor Red
        Write-Host "💡 Make sure the admin service is running and credentials are correct" -ForegroundColor Yellow
        throw
    }
}

# Function to seed users
function Invoke-UserSeeding {
    param(
        [string]$UserServiceUrl,
        [string]$Token,
        [int]$Count,
        [bool]$Clear
    )
    
    Write-Host "👥 Seeding $Count users..." -ForegroundColor Yellow
    
    try {
        $seedBody = @{
            count = $Count
            clearExisting = $Clear
            generateProfiles = $true
            includePortfolios = $true
            profilePictureStyle = "avataaars"
        } | ConvertTo-Json
        
        $headers = @{
            "Authorization" = "Bearer $Token"
            "Content-Type" = "application/json"
        }
        
        $response = Invoke-RestMethod -Uri "$UserServiceUrl/api/admin/seed-users" -Method POST -Body $seedBody -Headers $headers
        
        Write-Host "✅ Successfully seeded users" -ForegroundColor Green
        Write-Host "  📊 Users created: $($response.usersCreated)" -ForegroundColor Cyan
        Write-Host "  🖼️  Profile pictures: $($response.profilePicturesGenerated)" -ForegroundColor Cyan
        
        return $response
    }
    catch {
        Write-Host "❌ User seeding failed: $($_.Exception.Message)" -ForegroundColor Red
        throw
    }
}

# Function to seed blogs
function Invoke-BlogSeeding {
    param(
        [string]$BlogServiceUrl,
        [string]$Token,
        [int]$Count,
        [bool]$Clear,
        [array]$AuthorIds
    )
    
    Write-Host "📝 Seeding $Count blogs..." -ForegroundColor Yellow
    
    try {
        $seedBody = @{
            count = $Count
            clearExisting = $Clear
            authorIds = $AuthorIds
            generateVariations = $true
            includeMarkdown = $true
            featuredImageStyle = "tech"
            tagsVariety = $true
        } | ConvertTo-Json
        
        $headers = @{
            "Authorization" = "Bearer $Token"
            "Content-Type" = "application/json"
        }
        
        $response = Invoke-RestMethod -Uri "$BlogServiceUrl/api/admin/seed-blogs" -Method POST -Body $seedBody -Headers $headers
        
        Write-Host "✅ Successfully seeded blogs" -ForegroundColor Green
        Write-Host "  📚 Blogs created: $($response.blogsCreated)" -ForegroundColor Cyan
        Write-Host "  🏷️  Unique tags: $($response.uniqueTags)" -ForegroundColor Cyan
        
        return $response
    }
    catch {
        Write-Host "❌ Blog seeding failed: $($_.Exception.Message)" -ForegroundColor Red
        throw
    }
}

# Main execution
try {
    # Check Azure login
    if (-not (Test-AzureLogin)) {
        exit 1
    }
    
    # Get service URLs
    $serviceUrls = Get-ServiceUrls -ResourceGroup $ResourceGroup
    
    if (-not $serviceUrls["UserService"] -or -not $serviceUrls["BlogService"] -or -not $serviceUrls["AdminService"]) {
        Write-Host "❌ Could not find all required services. Found:" -ForegroundColor Red
        $serviceUrls.GetEnumerator() | ForEach-Object { Write-Host "  $($_.Key): $($_.Value)" }
        exit 1
    }
    
    # Authenticate with admin service
    $adminToken = Get-AdminToken -AdminServiceUrl $serviceUrls["AdminService"] -Email $AdminEmail -Password $AdminPassword
    
    # Seed users first
    $userResult = Invoke-UserSeeding -UserServiceUrl $serviceUrls["UserService"] -Token $adminToken -Count $Users -Clear $ClearExisting
    
    # Extract author IDs for blog seeding
    $authorIds = $userResult.createdUserIds
    if (-not $authorIds -or $authorIds.Count -eq 0) {
        Write-Host "⚠️  No author IDs returned, blogs will use random distribution" -ForegroundColor Yellow
        $authorIds = @()
    }
    
    # Seed blogs
    $blogResult = Invoke-BlogSeeding -BlogServiceUrl $serviceUrls["BlogService"] -Token $adminToken -Count $Blogs -Clear $ClearExisting -AuthorIds $authorIds
    
    Write-Host ""
    Write-Host "🎉 Data seeding completed successfully!" -ForegroundColor Green
    Write-Host "========================================" -ForegroundColor Green
    Write-Host "📊 Final Statistics:" -ForegroundColor White
    Write-Host "  👥 Users: $($userResult.usersCreated)" -ForegroundColor Cyan
    Write-Host "  📝 Blogs: $($blogResult.blogsCreated)" -ForegroundColor Cyan
    Write-Host "🌐 Frontend URL: $($serviceUrls["Frontend"])" -ForegroundColor Magenta
    Write-Host ""
    Write-Host "💡 You can now visit your frontend to see the populated data!" -ForegroundColor Yellow
    
}
catch {
    Write-Host ""
    Write-Host "❌ Seeding failed: $($_.Exception.Message)" -ForegroundColor Red
    Write-Host "Please check the error details above and try again." -ForegroundColor Yellow
    exit 1
}

