# Setup Guide - Deploy to Azure

This guide walks you through setting up the complete infrastructure and deployment pipeline.

## Prerequisites

Before you begin, ensure you have:

- ✅ **Azure Subscription** with contributor access
- ✅ **Azure CLI** installed (`az --version`)
- ✅ **Terraform** >= 1.5.0 installed (`terraform --version`)
- ✅ **.NET 9.0 SDK** installed (`dotnet --version`)
- ✅ **GitHub Account** with repository access
- ✅ **Git** installed

## 🚀 Step-by-Step Setup

### Step 1: Azure Authentication

```bash
# Login to Azure
az login

# Set your subscription (if you have multiple)
az account set --subscription "Your-Subscription-Name"

# Verify you're in the right subscription
az account show
```

### Step 2: Create Azure Service Principal for GitHub Actions

```bash
# Get your subscription ID
SUBSCRIPTION_ID=$(az account show --query id -o tsv)

# Create service principal with contributor role
az ad sp create-for-rbac \
  --name "github-actions-logmw" \
  --role contributor \
  --scopes /subscriptions/$SUBSCRIPTION_ID \
  --sdk-auth

# Save the JSON output - you'll need it for GitHub Secrets
```

**Output example:**
```json
{
  "clientId": "12345678-1234-1234-1234-123456789abc",
  "clientSecret": "your-client-secret",
  "subscriptionId": "your-subscription-id",
  "tenantId": "your-tenant-id",
  "activeDirectoryEndpointUrl": "https://login.microsoftonline.com",
  "resourceManagerEndpointUrl": "https://management.azure.com/",
  "activeDirectoryGraphResourceId": "https://graph.windows.net/",
  "sqlManagementEndpointUrl": "https://management.core.windows.net:8443/",
  "galleryEndpointUrl": "https://gallery.azure.com/",
  "managementEndpointUrl": "https://management.core.windows.net/"
}
```

### Step 3: Configure GitHub Secrets

1. Go to your GitHub repository
2. Navigate to **Settings** → **Secrets and variables** → **Actions**
3. Click **New repository secret**
4. Add the following secret:

| Secret Name | Value |
|-------------|-------|
| `AZURE_CREDENTIALS` | The entire JSON output from Step 2 |

### Step 4: Verify Workflow is in Place

The workflow should already be at `.github/workflows/deploy-blue-green.yml`.

Verify it exists:

```bash
# Check if workflow exists
ls -la .github/workflows/deploy-blue-green.yml

# Commit and push
git add .github/workflows/
git commit -m "chore: add deployment workflow"
git push origin main
```

### Step 5: Deploy Infrastructure (First Time)

You can deploy infrastructure either via GitHub Actions or locally:

#### Option A: Via GitHub Actions (Recommended)

```bash
# Push to main branch - pipeline will provision infrastructure
git push origin main

# Or trigger manually:
# 1. Go to GitHub → Actions tab
# 2. Select "Deploy to Azure App Service (Blue-Green)"
# 3. Click "Run workflow"
# 4. Select "dev" environment
# 5. Click "Run workflow"
```

#### Option B: Locally with Terraform

```bash
# Navigate to dev environment
cd infrastructure/terraform/environments/dev

# Initialize Terraform
terraform init

# Review the plan
terraform plan

# Apply (create resources)
terraform apply

# Note the outputs
terraform output
```

### Step 6: Verify Infrastructure

```bash
# List resources in the resource group
az resource list \
  --resource-group rg-logmw-dev \
  --output table

# You should see:
# - App Service Plan (asp-logmw-dev)
# - App Service (app-logmw-dev)
# - Application Insights (appi-logmw-dev)
# - Log Analytics Workspace (log-logmw-dev)
```

### Step 7: Verify Deployment Slots

```bash
# List deployment slots
az webapp deployment slot list \
  --name app-logmw-dev \
  --resource-group rg-logmw-dev \
  --output table

# You should see:
# - production (default)
# - green
```

### Step 8: Test Deployment

```bash
# Make a small change to trigger deployment
echo "# Test deployment" >> app/README.md

# Commit and push
git add .
git commit -m "test: trigger deployment"
git push origin main

# Watch the pipeline in GitHub Actions
```

### Step 9: Monitor Pipeline Execution

1. Go to **GitHub** → **Actions** tab
2. Click on the running workflow
3. Watch each stage:
   - ✅ Stage 1: Build Application
   - ✅ Stage 2: Provision Infrastructure
   - ✅ Stage 3: Deploy to Green Slot
   - ✅ Stage 4: Regression Tests
   - ✅ Stage 5: Swap to Production
   - ⏭️ Stage 6: Rollback (skipped if successful)

### Step 10: Verify Deployment

```bash
# Get your app URL
APP_NAME="app-logmw-dev"

# Test production slot
curl https://$APP_NAME.azurewebsites.net/health

# Test green slot
curl https://$APP_NAME-green.azurewebsites.net/health

# Open in browser
open https://$APP_NAME.azurewebsites.net/swagger
```

## 🧪 Testing the Deployment

### Test Health Endpoint

```bash
curl https://app-logmw-dev.azurewebsites.net/health
```

Expected output:
```json
{
  "status": "Healthy",
  "timestamp": "2025-11-12T...",
  "environment": "Development"
}
```

### Test API Endpoints

```bash
# Get orders
curl https://app-logmw-dev.azurewebsites.net/api/orders

# Get payments
curl https://app-logmw-dev.azurewebsites.net/api/payments
```

### Test Obfuscation (Check App Insights)

```bash
# Send request with sensitive data
curl -X POST https://app-logmw-dev.azurewebsites.net/api/payments/process \
  -H "Content-Type: application/json" \
  -d '{
    "orderId": 1,
    "amount": 99.99,
    "method": "CreditCard",
    "creditCard": "1234-5678-9012-3456",
    "cvv": "123"
  }'

# Check Application Insights logs in Azure Portal
# Logs should show: "creditCard": "***REDACTED***", "cvv": "***REDACTED***"
```

## 🔧 Manual Operations

### Manual Slot Swap

```bash
# Set environment variables
export RESOURCE_GROUP="rg-logmw-dev"
export APP_SERVICE_NAME="app-logmw-dev"

# Swap slots
./devops/scripts/swap-slots.sh
```

### Manual Validation

```bash
# Validate deployment
export BASE_URL="https://app-logmw-dev.azurewebsites.net"
./devops/scripts/validate-deployment.sh
```

### View Application Logs

```bash
# Stream logs from production slot
az webapp log tail \
  --name app-logmw-dev \
  --resource-group rg-logmw-dev

# Stream logs from green slot
az webapp log tail \
  --name app-logmw-dev \
  --slot green \
  --resource-group rg-logmw-dev
```

## 📊 Monitoring in Azure Portal

### View Application Insights

1. Go to **Azure Portal**
2. Navigate to **Resource Groups** → **rg-logmw-dev**
3. Click on **appi-logmw-dev** (Application Insights)
4. Explore:
   - **Live Metrics** - Real-time monitoring
   - **Logs** - Query custom properties
   - **Failures** - Error tracking
   - **Performance** - Response times

### Query Obfuscated Logs

In Application Insights → Logs, run:

```kusto
traces
| where customDimensions has "ObfuscatedRequest"
| project 
    timestamp,
    customDimensions.RequestPath,
    customDimensions.ObfuscatedRequest,
    customDimensions.ObfuscatedResponse
| order by timestamp desc
| take 20
```

## 🗑️ Cleanup (Optional)

To delete all Azure resources:

```bash
# Option 1: Via Terraform
cd infrastructure/terraform/environments/dev
terraform destroy

# Option 2: Via Azure CLI
az group delete --name rg-logmw-dev --yes --no-wait
```

## 🐛 Troubleshooting

### Pipeline Fails at Stage 2 (Infrastructure)

```bash
# Check Azure CLI authentication
az account show

# Verify service principal permissions
az role assignment list --assignee <clientId> --output table

# Test Terraform locally
cd infrastructure/terraform/environments/dev
terraform init
terraform validate
terraform plan
```

### Pipeline Fails at Stage 3 (Deploy)

```bash
# Check if App Service exists
az webapp show --name app-logmw-dev --resource-group rg-logmw-dev

# Check deployment credentials
az webapp deployment list-publishing-credentials \
  --name app-logmw-dev \
  --resource-group rg-logmw-dev
```

### Pipeline Fails at Stage 4 (Tests)

```bash
# Test endpoints manually
curl https://app-logmw-dev-green.azurewebsites.net/health -v

# Check App Service logs
az webapp log tail --name app-logmw-dev --slot green --resource-group rg-logmw-dev
```

### Application Not Starting

```bash
# Check App Service diagnostics
az webapp show \
  --name app-logmw-dev \
  --resource-group rg-logmw-dev \
  --query "{state:state, hostNames:defaultHostName}"

# Restart the app
az webapp restart --name app-logmw-dev --resource-group rg-logmw-dev
```

## ✅ Success Checklist

- [ ] Azure CLI authenticated
- [ ] Service principal created
- [ ] GitHub secret configured
- [ ] Workflow copied to `.github/workflows/`
- [ ] Infrastructure provisioned
- [ ] Deployment slots created
- [ ] Application deployed
- [ ] Health endpoint returns 200
- [ ] API endpoints working
- [ ] Application Insights receiving data
- [ ] Obfuscation working in logs

## 🎉 Next Steps

Once deployed successfully:

1. **Monitor** Application Insights for logs and metrics
2. **Test** obfuscation with sensitive data
3. **Explore** Swagger UI at `https://app-logmw-dev.azurewebsites.net/swagger`
4. **Add** staging/production environments
5. **Configure** custom domains (optional)
6. **Set up** alerts in Application Insights
7. **Add** authentication (Azure AD, API keys, etc.)

## 📚 Additional Resources

- [Azure App Service Docs](https://docs.microsoft.com/azure/app-service/)
- [Terraform Azure Provider](https://registry.terraform.io/providers/hashicorp/azurerm/latest/docs)
- [GitHub Actions Docs](https://docs.github.com/actions)
- [Application Insights Docs](https://docs.microsoft.com/azure/azure-monitor/app/app-insights-overview)
