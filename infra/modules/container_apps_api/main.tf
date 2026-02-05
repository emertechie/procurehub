resource "azurerm_container_app_environment" "this" {
  name                = "cae-${var.name_prefix}-${var.env}"
  location            = var.location
  resource_group_name = var.resource_group_name

  tags = var.tags
}

resource "azurerm_container_app" "this" {
  name                         = "ca-${var.name_prefix}-api-${var.env}"
  container_app_environment_id = azurerm_container_app_environment.this.id
  resource_group_name          = var.resource_group_name
  revision_mode                = "Single"

  identity {
    type = "SystemAssigned"
  }

  template {
    min_replicas = var.min_replicas
    max_replicas = var.max_replicas

    container {
      name   = "api"
      image  = var.container_image
      cpu    = var.cpu
      memory = var.memory

      env {
        name  = "ASPNETCORE_ENVIRONMENT"
        value = "Production"
      }

      env {
        name  = "MIGRATE_DB_ON_STARTUP"
        value = tostring(var.migrate_db_on_startup)
      }

      env {
        name  = "AllowedOrigins__0"
        value = length(var.allowed_origins) > 0 ? var.allowed_origins[0] : ""
      }

      env {
        name  = "ConnectionStrings__DefaultConnection"
        value = "Server=tcp:${var.sql_server_fqdn},1433;Database=${var.sql_database_name};User Id=${var.sql_admin_login};Encrypt=True;TrustServerCertificate=False;"
      }

      env {
        name        = "DatabasePassword"
        secret_name = "sql-password"
      }

      env {
        name  = "SEED_DATA"
        value = tostring(var.seed_data)
      }

      env {
        name  = "ENABLE_DEMO_MODE"
        value = tostring(var.enable_demo_mode)
      }

      liveness_probe {
        transport               = "HTTP"
        path                    = "/health"
        port                    = var.container_port
        initial_delay           = 10
        interval_seconds        = 30
        timeout                 = 5
        failure_count_threshold = 3
      }

      readiness_probe {
        transport               = "HTTP"
        path                    = "/health/ready"
        port                    = var.container_port
        interval_seconds        = 10
        timeout                 = 5
        failure_count_threshold = 3
      }
    }
  }

  secret {
    name                = "sql-password"
    key_vault_secret_id = "${var.key_vault_uri}secrets/sql-admin-password"
    identity            = "System"
  }

  ingress {
    external_enabled = true
    target_port      = var.container_port

    traffic_weight {
      percentage      = 100
      latest_revision = true
    }
  }

  tags = var.tags

  lifecycle {
    ignore_changes = [
      template[0].container[0].image # Managed by CI/CD pipeline
    ]
  }
}

resource "azurerm_role_assignment" "container_app_kv_secrets_user" {
  scope                = var.key_vault_id
  role_definition_name = "Key Vault Secrets User"
  principal_id         = azurerm_container_app.this.identity[0].principal_id
}
