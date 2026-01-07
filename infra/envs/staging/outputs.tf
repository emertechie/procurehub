output "resource_group_name" {
  description = "Resource group name"
  value       = module.rg.name
}

output "key_vault_name" {
  description = "Key Vault name"
  value       = module.key_vault.name
}

output "key_vault_uri" {
  description = "Key Vault URI"
  value       = module.key_vault.vault_uri
}

output "postgres_server_fqdn" {
  description = "PostgreSQL server FQDN"
  value       = module.postgres.server_fqdn
}

output "postgres_database_name" {
  description = "PostgreSQL database name"
  value       = module.postgres.database_name
}

output "container_app_url" {
  description = "Container App URL"
  value       = module.container_app.container_app_url
}

output "container_app_identity_principal_id" {
  description = "Container App managed identity principal ID"
  value       = module.container_app.container_app_identity_principal_id
}

output "github_actions_client_id" {
  description = "GitHub Actions OIDC Client ID (AZURE_CLIENT_ID)"
  value       = module.github_oidc.client_id
}

output "static_web_app_url" {
  description = "Static Web App default hostname"
  value       = module.static_web_app.default_host_name
}

output "static_web_app_api_key" {
  description = "Static Web App deployment token"
  value       = module.static_web_app.api_key
  sensitive   = true
}

# Observability outputs commented for now
# output "log_analytics_workspace_id" {
#   description = "Log Analytics workspace ID"
#   value       = module.observability.log_analytics_workspace_id
# }

# output "application_insights_connection_string" {
#   description = "Application Insights connection string"
#   value       = module.observability.application_insights_connection_string
#   sensitive   = true
# }

# output "application_insights_instrumentation_key" {
#   description = "Application Insights instrumentation key"
#   value       = module.observability.application_insights_instrumentation_key
#   sensitive   = true
# }
