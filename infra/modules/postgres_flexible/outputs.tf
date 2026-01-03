output "server_id" {
  description = "PostgreSQL server ID"
  value       = azurerm_postgresql_flexible_server.this.id
}

output "server_name" {
  description = "PostgreSQL server name"
  value       = azurerm_postgresql_flexible_server.this.name
}

output "server_fqdn" {
  description = "PostgreSQL server FQDN"
  value       = azurerm_postgresql_flexible_server.this.fqdn
}

output "database_name" {
  description = "Database name"
  value       = azurerm_postgresql_flexible_server_database.this.name
}

output "connection_string" {
  description = "PostgreSQL connection string"
  value       = "Server=${azurerm_postgresql_flexible_server.this.fqdn};Database=${var.database_name};Port=5432;User Id=${var.administrator_login};Password=${var.administrator_password};Ssl Mode=Require;"
  sensitive   = true
}
