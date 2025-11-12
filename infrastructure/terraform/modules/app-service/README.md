# App Service Module

This module creates an Azure App Service with support for blue-green deployments using deployment slots.

## Resources Created

- **App Service Plan** (Linux)
- **App Service** (production slot)
- **Deployment Slot** (green slot) - optional

## Features

- ✅ Linux-based App Service with .NET 9.0
- ✅ HTTPS enforcement
- ✅ Health check endpoint configured
- ✅ Application logging enabled
- ✅ HTTP logs with 7-day retention
- ✅ Green deployment slot for blue-green deployments

## Usage

```hcl
module "app_service" {
  source = "../../modules/app-service"

  name                = "app-myapp-dev"
  resource_group_name = azurerm_resource_group.main.name
  location            = azurerm_resource_group.main.location
  sku_name            = "B1"
  
  app_settings = {
    "ASPNETCORE_ENVIRONMENT" = "Development"
    "ApplicationInsights__ConnectionString" = azurerm_application_insights.main.connection_string
  }
  
  enable_green_slot = true
  
  tags = {
    Environment = "dev"
  }
}
```

## Blue-Green Deployment Process

1. **Deploy to Green Slot** - Deploy new version to the green slot
2. **Test Green Slot** - Run smoke tests and validation
3. **Swap Slots** - Swap green → production (instant cutover)
4. **Rollback** - If issues, swap back (previous version in green slot)

## Inputs

| Name | Description | Type | Default | Required |
|------|-------------|------|---------|----------|
| name | Name of the App Service | string | - | yes |
| resource_group_name | Resource group name | string | - | yes |
| location | Azure region | string | - | yes |
| sku_name | App Service Plan SKU | string | B1 | no |
| app_settings | Application settings | map(string) | {} | no |
| enable_green_slot | Enable green slot | bool | true | no |
| tags | Resource tags | map(string) | {} | no |

## Outputs

| Name | Description |
|------|-------------|
| app_service_name | Name of the App Service |
| default_hostname | Production slot hostname |
| green_slot_hostname | Green slot hostname |
| app_service_plan_id | App Service Plan ID |
