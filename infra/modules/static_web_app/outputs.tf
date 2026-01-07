output "id" {
  description = "Static Web App ID"
  value       = azurerm_static_web_app.main.id
}

output "default_host_name" {
  description = "Default hostname for the Static Web App"
  value       = azurerm_static_web_app.main.default_host_name
}

output "api_key" {
  description = "Deployment token for the Static Web App"
  value       = azurerm_static_web_app.main.api_key
  sensitive   = true
}
