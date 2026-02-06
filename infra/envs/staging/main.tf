# Staging environment infrastructure
# Shared resources - always deployed regardless of hosting variant

locals {
  # Increment this version to rotate the SQL Server admin password
  sql_password_version = 1
}

module "rg" {
  source   = "../../modules/rg"
  name     = "rg-${var.name_prefix}-${var.env}"
  location = var.location
  tags     = var.tags
}

module "key_vault" {
  source               = "../../modules/key_vault"
  name_prefix          = var.name_prefix
  env                  = var.env
  resource_group_name  = module.rg.name
  location             = var.location
  sql_password_version = local.sql_password_version
  tags                 = var.tags
}

module "sql_server" {
  source                      = "../../modules/sql_server"
  name_prefix                 = var.name_prefix
  env                         = var.env
  resource_group_name         = module.rg.name
  location                    = var.location2
  administrator_password      = module.key_vault.sql_admin_password
  password_version            = module.key_vault.sql_password_version
  sku_name                    = "GP_S_Gen5_1"
  auto_pause_delay_in_minutes = 60
  min_capacity                = 1
  tags                        = var.tags
}

module "github_oidc" {
  source            = "../../modules/github_oidc"
  app_name          = "${var.name_prefix}-${var.env}"
  github_repo       = var.github_repo
  resource_group_id = module.rg.id
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
