terraform {
  required_version = ">= 1.13.5"

  required_providers {
    azurerm = {
      source  = "hashicorp/azurerm"
      version = "~> 4.52.0"
    }
  }

  backend "azurerm" {
    resource_group_name  = "rg-tfstate-shared-infra"
    storage_account_name = "statesharedinfrajyzjo0l2"
    container_name       = "tfstate"
    key                  = "middleware/environments/dev/terraform.tfstate"
  }

}

provider "azurerm" {
  features {}
}

# Local variables
locals {
  environment          = "dev"
  location             = var.location
  resource_name_prefix = "logmw-${local.environment}"

  common_tags = {
    Environment = local.environment
    Project     = "Azure Logging Middleware"
    ManagedBy   = "Terraform"
    Repository  = "azure-appservice-logging-middleware"
  }
}

# Resource Group
resource "azurerm_resource_group" "main" {
  name     = "rg-${local.resource_name_prefix}"
  location = local.location
  tags     = local.common_tags
}

# Log Analytics Workspace (for Application Insights)
resource "azurerm_log_analytics_workspace" "main" {
  name                = "log-${local.resource_name_prefix}"
  location            = azurerm_resource_group.main.location
  resource_group_name = azurerm_resource_group.main.name
  sku                 = "PerGB2018"
  retention_in_days   = 30 # Saves ~50% vs 30 days
  tags                = local.common_tags
}

# Application Insights
resource "azurerm_application_insights" "main" {
  name                = "appi-${local.resource_name_prefix}"
  location            = azurerm_resource_group.main.location
  resource_group_name = azurerm_resource_group.main.name
  workspace_id        = azurerm_log_analytics_workspace.main.id
  application_type    = "web"
  tags                = local.common_tags
}

# App Service Module (with production + green slots)
module "app_service" {
  source = "../../modules/app-service"

  name                = "app-${local.resource_name_prefix}"
  resource_group_name = azurerm_resource_group.main.name
  location            = azurerm_resource_group.main.location

  # App Service Plan configuration
  sku_name = var.app_service_sku

  # Application settings
  app_settings = {
    "ASPNETCORE_ENVIRONMENT"                 = "Development"
    "ApplicationInsights__ConnectionString"  = azurerm_application_insights.main.connection_string
    "ObfuscationMiddleware__Enabled"         = "true"
    "ObfuscationMiddleware__ObfuscationMask" = "***REDACTED***"
  }

  # Enable deployment slots
  enable_green_slot = true

  # Networking (optional - can add VNet integration later)
  # vnet_integration_enabled = false

  tags = local.common_tags
}
