# React variant: Container App API + Static Web App
# Controlled by var.deploy_react_variant

module "static_web_app" {
  count = var.deploy_react_variant ? 1 : 0

  source              = "../../modules/static_web_app"
  name_prefix         = var.name_prefix
  env                 = var.env
  resource_group_name = module.rg.name
  location            = var.location2
  tags                = var.tags
}

module "container_app" {
  count = var.deploy_react_variant ? 1 : 0

  source                 = "../../modules/container_apps_api"
  name_prefix            = var.name_prefix
  env                    = var.env
  resource_group_name    = module.rg.name
  location               = var.location
  key_vault_id           = module.key_vault.id
  key_vault_uri          = module.key_vault.vault_uri
  postgres_server_fqdn   = module.postgres.server_fqdn
  postgres_database_name = module.postgres.database_name
  allowed_origins        = ["https://${module.static_web_app[0].default_host_name}"]
  migrate_db_on_startup  = true
  seed_data              = true
  enable_demo_mode       = true
  tags                   = var.tags
}
