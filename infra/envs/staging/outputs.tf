# Shared outputs - always available
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

output "github_actions_client_id" {
  description = "GitHub Actions OIDC Client ID (AZURE_CLIENT_ID)"
  value       = module.github_oidc.client_id
}

# React variant outputs - only available when deploy_react_variant = true
output "container_app_url" {
  description = "Container App URL"
  value       = var.deploy_react_variant ? module.container_app[0].container_app_url : null
}

output "container_app_identity_principal_id" {
  description = "Container App managed identity principal ID"
  value       = var.deploy_react_variant ? module.container_app[0].container_app_identity_principal_id : null
}

output "static_web_app_url" {
  description = "Static Web App default hostname"
  value       = var.deploy_react_variant ? module.static_web_app[0].default_host_name : null
}

output "static_web_app_api_key" {
  description = "Static Web App deployment token"
  value       = var.deploy_react_variant ? module.static_web_app[0].api_key : null
  sensitive   = true
}

# Blazor variant outputs - only available when deploy_blazor_variant = true
output "app_service_url" {
  description = "App Service URL"
  value       = var.deploy_blazor_variant ? module.app_service_blazor[0].app_url : null
}

output "app_service_name" {
  description = "App Service name"
  value       = var.deploy_blazor_variant ? module.app_service_blazor[0].app_name : null
}

output "app_service_identity_principal_id" {
  description = "App Service managed identity principal ID"
  value       = var.deploy_blazor_variant ? module.app_service_blazor[0].app_identity_principal_id : null
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
