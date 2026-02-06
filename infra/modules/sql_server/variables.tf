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
  default     = "sqladmin"
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
  description = "SKU name (e.g., Basic, S0, S1, GP_S_Gen5_1 for serverless)"
  type        = string
  default     = "Basic"
}

variable "auto_pause_delay_in_minutes" {
  description = "Time in minutes before database auto-pauses. -1 to disable. Only applies to serverless SKUs."
  type        = number
  default     = -1
}

variable "min_capacity" {
  description = "Minimum vCore capacity when active. Only applies to serverless SKUs."
  type        = number
  default     = null
}

variable "backup_retention_days" {
  description = "Short-term backup retention in days (1-35)"
  type        = number
  default     = 7
}

variable "tags" {
  description = "Tags to apply to resources"
  type        = map(string)
  default     = {}
}
