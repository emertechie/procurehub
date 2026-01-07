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
