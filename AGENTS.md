# General

- In all interactions, plans, and commit messages, be extremely concise and sacrifice grammar for the sake of concision.
- Never say "Great question!" or "You're absolutely right!" or similar when replying to questions.

# Application Context

- For detailed project overview, see: `.context/procure_hub_project_overview.md`
- For list of possible app use cases, see: `.context/all_use_cases.md`

# Tech Stack

- This is a full stack app using .Net (C#) for backend API and React (with TanStack Router) for frontend SPA
- The API uses ASP.Net Core Minimal APIs, EF Core with Postgres DB, and modern C# code using .Net Core version 10
- The API generates an OpenAPI spec which is converted to a strongly typed `openapi-react-query` client for the web project
  - Update the client after any API change that updates the OpenAPI spec. Use command given below.
- Detailed guidance for the React app can be found in `ProcureHub.WebApp/AGENTS.md`

# Architecture

- On both backend and frontend, implement features using a Vertical Slice Architecture (VSA). Use `/Features/{FeatureName}` folders to group all code for a feature together.
- Use command-focused endpoints in API design: Use slim update endpoints for basic profile edits (user name, department name). Use dedicated command endpoints for actions (enable/disable user, assign department, assign roles).

# Project Structure

## .Net Backend

- `/ProcureHub`: The core .Net domain project. Keep dependencies to a minimum.
  - `/Features`: VSA feature folders
  - `/Models`: EF Core entity models and `IEntityTypeConfiguration` types (keep both in the same file).
  - `/Migrations`: EF Core migrations. (See command below for how to update)
- `/ProcureHub.WebApi`: The ASP.Net Core Minimal API project
  - `Program.cs` - Configures and runs API host
  - `ApiEndpoints.cs` - root class for configuring API endpoints (calls out to specific extensions defined in feature folders)
  - `/Features`: VSA feature folders. Provides extension methods on `WebApplication` to configure feature-specific endpoints
- `/ProcureHub.WebApi.Tests`: API tests project using Xunit v3.
  - `/Features`: add feature-specific tests here

## React Frontend

- `/ProcureHub.WebApp`: Project structure and feature layout rules are defined in `ProcureHub.WebApp/docs/project-structure.md`

# Testing

- You must add or update API tests when adding or updating any API endpoint
  - Use the `ProcureHub.WebApi.Tests/Features/UserTests.cs` file as a guide for how to implement tests

# Commons Commands

- Update EF Core migrations: `dotnet ef migrations add ExampleMigration -p ../ProcureHub`
  - NOTE: Run all migration commands in the `ProcureHub.WebApi` dir, and add the `-p` flag for project reference as shown above
- Update the strongly typed `openapi-react-query` client after a change to OpenAPI spec: In the `/ProcureHub.WebApp` folder, run `npm run generate:api-schema`
