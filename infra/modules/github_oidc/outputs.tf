output "client_id" {
  description = "GitHub Actions OIDC client ID (AZURE_CLIENT_ID)"
  value       = azuread_application.github_actions.client_id
}

output "application_id" {
  description = "GitHub Actions application ID"
  value       = azuread_application.github_actions.id
}

output "service_principal_id" {
  description = "GitHub Actions service principal object ID"
  value       = azuread_service_principal.github_actions.id
}
