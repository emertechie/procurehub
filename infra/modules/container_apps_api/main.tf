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

      # Database connection env vars commented out until deploying actual API
      # env {
      #   name  = "ConnectionStrings__DefaultConnection"
      #   value = "Server=${var.postgres_server_fqdn};Database=${var.postgres_database_name};Port=5432;User Id=${var.postgres_admin_login};Ssl Mode=Require;"
      # }

      # env {
      #   name        = "ConnectionStrings__Password"
      #   secret_name = "postgres-password"
      # }
    }
  }

  # Secret reference commented out until deploying actual API
  # secret {
  #   name                = "postgres-password"
  #   key_vault_secret_id = "${var.key_vault_uri}secrets/postgres-admin-password"
  #   identity            = "System"
  # }

  ingress {
    external_enabled = true
    target_port      = var.container_port

    traffic_weight {
      percentage      = 100
      latest_revision = true
    }
  }

  tags = var.tags
}

# Role assignment for Container App managed identity to access Key Vault secrets
# Must be done manually - service principal lacks roleAssignments/write permission
