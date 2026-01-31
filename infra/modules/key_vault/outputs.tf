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

output "postgres_admin_password_secret_name" {
  description = "Name of the postgres admin password secret in Key Vault"
  value       = azurerm_key_vault_secret.postgres_admin_password.name
}

output "postgres_password_version" {
  description = "Current version of postgres admin password"
  value       = var.postgres_password_version
}

output "postgres_admin_password" {
  description = "Ephemeral postgres admin password (not stored in state)"
  ephemeral   = true
  value       = ephemeral.random_password.postgres_admin.result
}
