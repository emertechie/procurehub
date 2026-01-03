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
