# App Service Plan
resource "azurerm_service_plan" "main" {
  name                = "asp-${var.name}"
  location            = var.location
  resource_group_name = var.resource_group_name
  os_type             = "Linux"
  sku_name            = var.sku_name
  tags                = var.tags
}

# App Service (Production Slot)
resource "azurerm_linux_web_app" "main" {
  name                = var.name
  location            = var.location
  resource_group_name = var.resource_group_name
  service_plan_id     = azurerm_service_plan.main.id
  https_only          = true
  tags                = var.tags

  site_config {
    always_on        = true
    http2_enabled    = true
    
    application_stack {
      dotnet_version = "9.0"
    }

    # Health check configuration
    health_check_path = "/health"
    health_check_eviction_time_in_min = "2"
    
    # CORS settings (adjust as needed)
    cors {
      allowed_origins = ["*"]  # Tighten this in production
    }
  }

  app_settings = var.app_settings

  logs {
    application_logs {
      file_system_level = "Information"
    }
    
    http_logs {
      file_system {
        retention_in_days = 7
        retention_in_mb   = 35
      }
    }
  }

  lifecycle {
    ignore_changes = [
      # Ignore changes to app_settings that might be set via deployment
      app_settings["WEBSITE_RUN_FROM_PACKAGE"],
    ]
  }
}

# Green Deployment Slot (for blue-green deployments)
resource "azurerm_linux_web_app_slot" "green" {
  count = var.enable_green_slot ? 1 : 0

  name           = "green"
  app_service_id = azurerm_linux_web_app.main.id
  https_only     = true
  tags           = var.tags

  site_config {
    always_on        = true
    http2_enabled    = true
    
    application_stack {
      dotnet_version = "9.0"
    }

    # Health check configuration
    health_check_path = "/health"
    health_check_eviction_time_in_min = "2"
 
    
    # CORS settings
    cors {
      allowed_origins = ["*"]  # Tighten this in production
    }

    # Auto swap configuration (disabled by default for manual control)
    # auto_swap_slot_name = "production"
  }

  app_settings = merge(
    var.app_settings,
    {
      "ASPNETCORE_ENVIRONMENT" = "Staging"  # Override for green slot
    }
  )

  logs {
    application_logs {
      file_system_level = "Information"
    }
    
    http_logs {
      file_system {
        retention_in_days = 7
        retention_in_mb   = 35
      }
    }
  }
}
