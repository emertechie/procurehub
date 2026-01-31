# Staging environment infrastructure
# Shared resources - always deployed regardless of hosting variant

locals {
  # Increment this version to rotate the postgres admin password
  postgres_password_version = 1
}

module "rg" {
  source   = "../../modules/rg"
  name     = "rg-${var.name_prefix}-${var.env}"
  location = var.location
  tags     = var.tags
}

module "key_vault" {
  source                    = "../../modules/key_vault"
  name_prefix               = var.name_prefix
  env                       = var.env
  resource_group_name       = module.rg.name
  location                  = var.location
  postgres_password_version = local.postgres_password_version
  tags                      = var.tags
}

module "postgres" {
  source                 = "../../modules/postgres_flexible"
  name_prefix            = var.name_prefix
  env                    = var.env
  resource_group_name    = module.rg.name
  location               = var.location
  administrator_password = module.key_vault.postgres_admin_password
  password_version       = module.key_vault.postgres_password_version
  tags                   = var.tags
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
