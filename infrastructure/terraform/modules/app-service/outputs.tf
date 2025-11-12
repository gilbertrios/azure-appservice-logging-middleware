output "app_service_id" {
  description = "ID of the App Service"
  value       = azurerm_linux_web_app.main.id
}

output "app_service_name" {
  description = "Name of the App Service"
  value       = azurerm_linux_web_app.main.name
}

output "default_hostname" {
  description = "Default hostname of the App Service (production slot)"
  value       = azurerm_linux_web_app.main.default_hostname
}

output "green_slot_hostname" {
  description = "Hostname of the green deployment slot"
  value       = var.enable_green_slot ? azurerm_linux_web_app_slot.green[0].default_hostname : null
}

output "app_service_plan_id" {
  description = "ID of the App Service Plan"
  value       = azurerm_service_plan.main.id
}

output "outbound_ip_addresses" {
  description = "Outbound IP addresses of the App Service"
  value       = azurerm_linux_web_app.main.outbound_ip_addresses
}
