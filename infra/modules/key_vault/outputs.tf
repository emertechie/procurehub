output "id" {
  description = "Key Vault ID"
  value       = azurerm_key_vault.this.id
}

output "name" {
  description = "Key Vault name"
  value       = azurerm_key_vault.this.name
}

output "vault_uri" {
  description = "Key Vault URI"
  value       = azurerm_key_vault.this.vault_uri
}

output "sql_admin_password_secret_name" {
  description = "Name of the SQL Server admin password secret in Key Vault"
  value       = azurerm_key_vault_secret.sql_admin_password.name
}

output "sql_admin_password_secret_uri_with_version" {
  description = "Versioned URI for SQL Server admin password secret"
  value       = azurerm_key_vault_secret.sql_admin_password.id
}

output "sql_password_version" {
  description = "Current version of SQL Server admin password"
  value       = var.sql_password_version
}

output "sql_admin_password" {
  description = "Ephemeral SQL Server admin password (not stored in state)"
  ephemeral   = true
  value       = ephemeral.random_password.sql_admin.result
}
