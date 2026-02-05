output "server_id" {
  description = "SQL Server ID"
  value       = azurerm_mssql_server.this.id
}

output "server_name" {
  description = "SQL Server name"
  value       = azurerm_mssql_server.this.name
}

output "server_fqdn" {
  description = "SQL Server FQDN"
  value       = azurerm_mssql_server.this.fully_qualified_domain_name
}

output "database_name" {
  description = "Database name"
  value       = azurerm_mssql_database.this.name
}

output "connection_string" {
  description = "SQL Server connection string (without password - retrieve password from Key Vault)"
  value       = "Server=tcp:${azurerm_mssql_server.this.fully_qualified_domain_name},1433;Database=${var.database_name};User Id=${var.administrator_login};Encrypt=True;TrustServerCertificate=False;"
  sensitive   = true
}
