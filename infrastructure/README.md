# Infrastructure as Code (IaC)

This directory contains all Infrastructure as Code (IaC) for provisioning Azure resources.

## 🏗️ Architecture

### Dev Environment Resources

```
Resource Group: rg-logmw-dev
├── App Service Plan: asp-logmw-dev (Linux, S1)  # S1 required for deployment slots
├── App Service: app-logmw-dev
│   ├── Production Slot (default)
│   └── Green Slot (for blue-green deployments)
├── Application Insights: appi-logmw-dev
└── Log Analytics Workspace: log-logmw-dev (7-day retention)
```

## 📁 Structure

```
infrastructure/
├── terraform/
│   ├── environments/
│   │   └── dev/                    # Dev environment configuration
│   │       ├── main.tf             # Main resources
│   │       ├── variables.tf        # Input variables
│   │       ├── outputs.tf          # Output values
│   │       └── terraform.tfvars    # Dev-specific values
│   │
│   └── modules/
│       └── app-service/            # Reusable App Service module
│           ├── main.tf             # App Service + slots
│           ├── variables.tf
│           ├── outputs.tf
│           └── README.md
│
└── scripts/
    └── terraform-init.sh           # Helper script
```

## 🚀 Quick Start

### Prerequisites

1. **Azure CLI** installed and authenticated
2. **Terraform** >= 1.5.0 installed
3. **Azure subscription** with contributor access

### Deploy Dev Environment

```bash
# Login to Azure
az login

# Navigate to dev environment
cd infrastructure/terraform/environments/dev

# Initialize Terraform
terraform init

# Review planned changes
terraform plan

# Apply infrastructure
terraform apply

# View outputs
terraform output
```

## 🔧 Configuration

### Dev Environment (`terraform.tfvars`)

```hcl
location         = "eastus"
app_service_sku = "S1"  # Standard tier (required for deployment slots)
```

### Available SKUs

| SKU | Description | Deployment Slots | Use Case |
|-----|-------------|------------------|----------|
| B1 | Basic - 1 core, 1.75 GB RAM | ❌ No | Simple dev (no slots) |
| S1 | Standard - 1 core, 1.75 GB RAM | ✅ Yes | Dev/Staging with slots |
| P1v2 | Premium - 1 core, 3.5 GB RAM | ✅ Yes | Production |
| P2v2 | Premium - 2 cores, 7 GB RAM | ✅ Yes | Production (high traffic) |

**Note:** Deployment slots require Standard (S1) tier or higher.

## 📊 Outputs

After `terraform apply`, you'll get:

```bash
app_service_name              = "app-logmw-dev"
app_service_default_hostname  = "app-logmw-dev.azurewebsites.net"
app_service_green_hostname    = "app-logmw-dev-green.azurewebsites.net"
resource_group_name           = "rg-logmw-dev"
```

## 🔄 Blue-Green Deployment Slots

### Slot Configuration

- **Production Slot** - Current live version
- **Green Slot** - New version for testing

### Deployment Flow

```
1. Deploy → Green Slot (staging)
2. Test → Run smoke tests on green
3. Swap → Green becomes production (instant)
4. Rollback → Swap back if issues (previous version in green)
```

### Manual Slot Swap

```bash
# Swap green to production
az webapp deployment slot swap \
  --resource-group rg-logmw-dev \
  --name app-logmw-dev \
  --slot green \
  --target-slot production
```

## 🔐 State Management

### Remote State (Azure Storage Backend)

Terraform state is stored in Azure Storage for team collaboration and CI/CD:

```hcl
terraform {
  backend "azurerm" {
    resource_group_name  = "rg-tfstate-shared-infra"
    storage_account_name = "statesharedinfrajyzjo0l2"
    container_name       = "tfstate"
    key                  = "middleware/environments/dev/terraform.tfstate"
  }
}
```

**Benefits:**
- ✅ Team collaboration (shared state)
- ✅ State locking (prevents conflicts)
- ✅ Encrypted at rest
- ✅ Works with CI/CD pipelines

### State Management Commands

```bash
# View current state
terraform show

# List resources in state
terraform state list

# Remove resource from state (use with caution)
terraform state rm <resource>

# Pull remote state
terraform state pull
```

## 🧪 Validation

```bash
# Validate configuration
terraform validate

# Format code
terraform fmt -recursive

# Check for security issues (if using tfsec)
tfsec .
```

## 🗑️ Cleanup

```bash
# Destroy all resources (use with caution!)
cd infrastructure/terraform/environments/dev
terraform destroy
```

## 📝 Best Practices

✅ **Use modules** for reusable infrastructure  
✅ **Tag all resources** for cost tracking  
✅ **Enable diagnostics** on all services  
✅ **Use Key Vault** for secrets (add when needed)  
✅ **Enable App Service authentication** for production  
✅ **Configure alerts** in Application Insights  

## 🔗 Resources

- [Terraform Azure Provider](https://registry.terraform.io/providers/hashicorp/azurerm/latest/docs)
- [Azure App Service Docs](https://docs.microsoft.com/azure/app-service/)
- [Deployment Slots Best Practices](https://docs.microsoft.com/azure/app-service/deploy-best-practices)
