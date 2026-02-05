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
  description = "Key Vault ID for secret references"
  type        = string
}

variable "key_vault_uri" {
  description = "Key Vault URI for secret references"
  type        = string
}

variable "container_image" {
  description = "Container image to deploy"
  type        = string
  default     = "mcr.microsoft.com/azuredocs/containerapps-helloworld:latest"
}

variable "container_port" {
  description = "Container port for ingress"
  type        = number
  default     = 8080
}

variable "min_replicas" {
  description = "Minimum number of replicas"
  type        = number
  default     = 1
}

variable "max_replicas" {
  description = "Maximum number of replicas"
  type        = number
  default     = 1
}

variable "cpu" {
  description = "CPU cores (e.g., 0.25, 0.5, 1.0)"
  type        = number
  default     = 0.5
}

variable "memory" {
  description = "Memory in Gi (e.g., 0.5, 1.0, 2.0)"
  type        = string
  default     = "1Gi"
}

variable "sql_server_fqdn" {
  description = "SQL Server FQDN"
  type        = string
}

variable "sql_database_name" {
  description = "SQL Server database name"
  type        = string
}

variable "sql_admin_login" {
  description = "SQL Server admin username"
  type        = string
  default     = "sqladmin"
}

variable "tags" {
  description = "Tags to apply to resources"
  type        = map(string)
  default     = {}
}

variable "migrate_db_on_startup" {
  description = "Run EF Core migrations on container startup (true for staging, false for prod)"
  type        = bool
  default     = false
}

variable "allowed_origins" {
  description = "Allowed CORS origins (e.g., Static Web App URLs)"
  type        = list(string)
  default     = []
}

variable "seed_data" {
  description = "Seed data on startup (true for staging, false for prod)"
  type        = bool
  default     = false
}

variable "enable_demo_mode" {
  description = "Enable demo mode features (true for staging, false for prod)"
  type        = bool
  default     = false
}
