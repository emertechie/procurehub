# Staging environment infrastructure

module "rg" {
  source   = "../../modules/rg"
  name     = "rg-${var.name_prefix}-${var.env}"
  location = var.location
  tags     = var.tags
}

module "key_vault" {
  source              = "../../modules/key_vault"
  name_prefix         = var.name_prefix
  env                 = var.env
  resource_group_name = module.rg.name
  location            = var.location
  tags                = var.tags
}

# Read postgres password from Key Vault
data "azurerm_key_vault_secret" "postgres_password" {
  name         = "postgres-admin-password"
  key_vault_id = module.key_vault.id
}

module "postgres" {
  source                 = "../../modules/postgres_flexible"
  name_prefix            = var.name_prefix
  env                    = var.env
  resource_group_name    = module.rg.name
  location               = var.location
  administrator_password = data.azurerm_key_vault_secret.postgres_password.value
  tags                   = var.tags
}

# Observability module commented out for now
# module "observability" {
#   source              = "../../modules/log_analytics_appinsights"
#   name_prefix         = var.name_prefix
#   env                 = var.env
#   resource_group_name = module.rg.name
#   location            = var.location
#   tags                = var.tags
# }
