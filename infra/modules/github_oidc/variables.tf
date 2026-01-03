variable "app_name" {
  description = "Application name for OIDC app display name"
  type        = string
}

variable "github_repo" {
  description = "GitHub repository in format 'owner/repo'"
  type        = string
}

variable "resource_group_id" {
  description = "Resource group ID to grant Contributor access"
  type        = string
}
