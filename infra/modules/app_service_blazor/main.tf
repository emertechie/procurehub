resource "azurerm_service_plan" "this" {
  name                = "asp-${var.name_prefix}-${var.env}"
  location            = var.location
  resource_group_name = var.resource_group_name
  os_type             = "Linux"
  sku_name            = var.sku_name

  tags = var.tags
}

resource "azurerm_linux_web_app" "this" {
  name                = "app-${var.name_prefix}-${var.env}"
  location            = var.location
  resource_group_name = var.resource_group_name
  service_plan_id     = azurerm_service_plan.this.id

  https_only = true

  identity {
    type = "SystemAssigned"
  }

  site_config {
    always_on = var.sku_name != "F1" && var.sku_name != "B1" ? true : false

    application_stack {
      dotnet_version = var.dotnet_version
    }

    health_check_path                 = "/health"
    health_check_eviction_time_in_min = 5
  }

  app_settings = {
    "ASPNETCORE_ENVIRONMENT"               = "Production"
    "MIGRATE_DB_ON_STARTUP"                = tostring(var.migrate_db_on_startup)
    "SEED_DATA"                            = tostring(var.seed_data)
    "ENABLE_DEMO_MODE"                     = tostring(var.enable_demo_mode)
    "ConnectionStrings__DefaultConnection" = "Host=${var.postgres_server_fqdn};Database=${var.postgres_database_name};Port=5432;Username=${var.postgres_admin_login};Ssl Mode=Require"
    "DatabasePassword"                     = "@Microsoft.KeyVault(SecretUri=${var.postgres_admin_password_secret_uri_with_version})"
  }

  tags = var.tags
}

resource "azurerm_role_assignment" "app_kv_secrets_user" {
  scope                = var.key_vault_id
  role_definition_name = "Key Vault Secrets User"
  principal_id         = azurerm_linux_web_app.this.identity[0].principal_id
}
