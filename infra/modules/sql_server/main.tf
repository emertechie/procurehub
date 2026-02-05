resource "azurerm_mssql_server" "this" {
  name                = "sql-${var.name_prefix}-${var.env}"
  resource_group_name = var.resource_group_name
  location            = var.location
  version             = "12.0"

  administrator_login                     = var.administrator_login
  administrator_login_password_wo         = var.administrator_password
  administrator_login_password_wo_version = var.password_version

  minimum_tls_version = "1.2"

  tags = var.tags
}

resource "azurerm_mssql_database" "this" {
  name      = var.database_name
  server_id = azurerm_mssql_server.this.id
  collation = "SQL_Latin1_General_CP1_CI_AS"
  sku_name  = var.sku_name

  short_term_retention_policy {
    retention_days = var.backup_retention_days
  }

  tags = var.tags
}

# Allow Azure services to access
resource "azurerm_mssql_firewall_rule" "allow_azure_services" {
  name             = "AllowAzureServices"
  server_id        = azurerm_mssql_server.this.id
  start_ip_address = "0.0.0.0"
  end_ip_address   = "0.0.0.0"
}
