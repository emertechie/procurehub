data "azurerm_client_config" "current" {}

resource "azurerm_key_vault" "this" {
  name                       = "kv-${var.name_prefix}-${var.env}"
  location                   = var.location
  resource_group_name        = var.resource_group_name
  tenant_id                  = data.azurerm_client_config.current.tenant_id
  sku_name                   = "standard"
  soft_delete_retention_days = 7
  purge_protection_enabled   = false

  rbac_authorization_enabled = true

  network_acls {
    bypass         = "AzureServices"
    default_action = "Allow"
  }

  tags = var.tags
}

resource "azurerm_role_assignment" "kv_admin" {
  scope                = azurerm_key_vault.this.id
  role_definition_name = "Key Vault Administrator"
  principal_id         = data.azurerm_client_config.current.object_id
}

# Generate ephemeral random password for postgres
ephemeral "random_password" "postgres_admin" {
  length           = 32
  special          = true
  override_special = "!#$%&*()-_=+[]{}<>:?"
}

# Store password in Key Vault using write-only argument
resource "azurerm_key_vault_secret" "postgres_admin_password" {
  name             = "postgres-admin-password"
  value_wo         = ephemeral.random_password.postgres_admin.result
  value_wo_version = var.postgres_password_version
  key_vault_id     = azurerm_key_vault.this.id

  depends_on = [azurerm_role_assignment.kv_admin]
}
