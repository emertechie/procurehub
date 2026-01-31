output "app_url" {
  description = "App Service default URL"
  value       = "https://${azurerm_linux_web_app.this.default_hostname}"
}

output "app_name" {
  description = "App Service name"
  value       = azurerm_linux_web_app.this.name
}

output "app_identity_principal_id" {
  description = "App Service managed identity principal ID"
  value       = azurerm_linux_web_app.this.identity[0].principal_id
}

output "service_plan_id" {
  description = "App Service Plan ID"
  value       = azurerm_service_plan.this.id
}
