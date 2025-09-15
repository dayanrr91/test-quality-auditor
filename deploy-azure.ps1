# Script para deploy en Azure App Service
# Requiere Azure CLI instalado: https://docs.microsoft.com/en-us/cli/azure/install-azure-cli

Write-Host "🚀 Deploying Test Quality Auditor to Azure..." -ForegroundColor Green

# Variables (cambia estos valores)
$resourceGroupName = "test-quality-auditor-rg"
$appServiceName = "test-quality-auditor-app"
$location = "East US"
$subscriptionId = "your-subscription-id"

# Login a Azure
Write-Host "🔐 Logging in to Azure..." -ForegroundColor Yellow
az login

# Seleccionar suscripción
Write-Host "📋 Setting subscription..." -ForegroundColor Yellow
az account set --subscription $subscriptionId

# Crear Resource Group
Write-Host "📦 Creating Resource Group..." -ForegroundColor Yellow
az group create --name $resourceGroupName --location $location

# Crear App Service Plan (Consumption)
Write-Host "🏗️ Creating App Service Plan..." -ForegroundColor Yellow
az appservice plan create --name "test-quality-auditor-plan" --resource-group $resourceGroupName --sku F1 --is-linux

# Crear Web App
Write-Host "🌐 Creating Web App..." -ForegroundColor Yellow
az webapp create --resource-group $resourceGroupName --plan "test-quality-auditor-plan" --name $appServiceName --runtime "DOTNET|8.0"

# Configurar para .NET 8
Write-Host "⚙️ Configuring .NET 8..." -ForegroundColor Yellow
az webapp config set --resource-group $resourceGroupName --name $appServiceName --net-framework-version "v8.0"

# Deploy desde GitHub
Write-Host "📤 Deploying from GitHub..." -ForegroundColor Yellow
az webapp deployment source config --resource-group $resourceGroupName --name $appServiceName --repo-url "https://github.com/tu-usuario/test-quality-auditor.git" --branch main --manual-integration

Write-Host "✅ Deploy completed!" -ForegroundColor Green
Write-Host "🌐 Your app is available at: https://$appServiceName.azurewebsites.net" -ForegroundColor Cyan
