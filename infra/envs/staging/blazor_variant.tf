# Blazor variant: App Service (Blazor SSR + API)
# Controlled by var.deploy_blazor_variant

module "app_service_blazor" {
  count = var.deploy_blazor_variant ? 1 : 0

  source                                          = "../../modules/app_service_blazor"
  name_prefix                                     = var.name_prefix
  env                                             = var.env
  resource_group_name                             = module.rg.name
  location                                        = var.location2
  key_vault_id                                    = module.key_vault.id
  key_vault_uri                                   = module.key_vault.vault_uri
  postgres_admin_password_secret_uri_with_version = module.key_vault.postgres_admin_password_secret_uri_with_version
  postgres_server_fqdn                            = module.postgres.server_fqdn
  postgres_database_name                          = module.postgres.database_name
  migrate_db_on_startup                           = true
  seed_data                                       = true
  enable_demo_mode                                = true
  tags                                            = var.tags
}
