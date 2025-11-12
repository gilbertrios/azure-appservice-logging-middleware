variable "location" {
  description = "Azure region for resources"
  type        = string
  default     = "eastus"
}

variable "app_service_sku" {
  description = "SKU for App Service Plan (B1, S1, P1v2, etc.)"
  type        = string
  default     = "B1"  # Basic tier - good for dev
}
