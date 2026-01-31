variable "name_prefix" {
  description = "Prefix for resource names"
  type        = string
}

variable "env" {
  description = "Environment name"
  type        = string
}

variable "resource_group_name" {
  description = "Resource group name"
  type        = string
}

variable "location" {
  description = "Azure region"
  type        = string
}

variable "key_vault_id" {
  description = "Key Vault ID for role assignment"
  type        = string
}

variable "key_vault_uri" {
  description = "Key Vault URI for secret references"
  type        = string
}

variable "postgres_admin_password_secret_uri_with_version" {
  description = "Versioned secret URI for postgres admin password"
  type        = string
}

variable "postgres_server_fqdn" {
  description = "PostgreSQL server FQDN"
  type        = string
}

variable "postgres_database_name" {
  description = "PostgreSQL database name"
  type        = string
}

variable "postgres_admin_login" {
  description = "PostgreSQL admin username"
  type        = string
  default     = "pgadmin"
}

variable "sku_name" {
  description = "App Service Plan SKU (e.g., B1, S1, P1v2)"
  type        = string
  default     = "B1"
}

variable "dotnet_version" {
  description = ".NET version for the app"
  type        = string
  default     = "10.0"
}

variable "migrate_db_on_startup" {
  description = "Run EF Core migrations on app startup"
  type        = bool
  default     = false
}

variable "seed_data" {
  description = "Seed data on startup"
  type        = bool
  default     = false
}

variable "enable_demo_mode" {
  description = "Enable demo mode features"
  type        = bool
  default     = false
}

variable "tags" {
  description = "Tags to apply to resources"
  type        = map(string)
  default     = {}
}
