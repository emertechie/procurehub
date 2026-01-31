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

variable "administrator_login" {
  description = "Administrator username"
  type        = string
  default     = "pgadmin"
}

variable "administrator_password" {
  description = "Ephemeral administrator password"
  type        = string
  ephemeral   = true
}

variable "password_version" {
  description = "Version number for administrator password"
  type        = number
}

variable "database_name" {
  description = "Database name to create"
  type        = string
  default     = "procurehub"
}

variable "sku_name" {
  description = "SKU name (e.g., B_Standard_B1ms for burstable)"
  type        = string
  default     = "B_Standard_B1ms"
}

variable "storage_mb" {
  description = "Storage size in MB"
  type        = number
  default     = 32768
}

variable "backup_retention_days" {
  description = "Backup retention in days"
  type        = number
  default     = 7
}

variable "tags" {
  description = "Tags to apply to resources"
  type        = map(string)
  default     = {}
}
