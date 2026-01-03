output "container_app_environment_id" {
  description = "Container App Environment ID"
  value       = azurerm_container_app_environment.this.id
}

output "container_app_id" {
  description = "Container App ID"
  value       = azurerm_container_app.this.id
}

output "container_app_fqdn" {
  description = "Container App FQDN"
  value       = azurerm_container_app.this.ingress[0].fqdn
}

output "container_app_url" {
  description = "Container App URL"
  value       = "https://${azurerm_container_app.this.ingress[0].fqdn}"
}

output "container_app_identity_principal_id" {
  description = "Container App managed identity principal ID"
  value       = azurerm_container_app.this.identity[0].principal_id
}
