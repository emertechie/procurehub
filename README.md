# ProcureHub

A full-stack procurement and purchase approvals system demonstrating enterprise-grade patterns, end-to-end type safety, and production-ready architecture.

> 🎯 **Purpose**: This project showcases real-world patterns I use in production applications—not a toy example, but a demonstration of how I structure, test, and deploy software.

---

## ✨ Key Highlights

| Area | What's Demonstrated |
|------|---------------------|
| **End-to-End Type Safety** | OpenAPI spec auto-generates a TypeScript client—types flow from C# API to React components |
| **Vertical Slice Architecture** | Features grouped cohesively on both [backend](ProcureHub/Features) and [frontend](ProcureHub.WebApp/src/features) |
| **Role-Based Access Control** | UI adapts per role; API enforces authorization at every endpoint |
| **Comprehensive Testing** | Integration tests ensure all endpoints are authenticated, authorized, and validated |
| **Infrastructure as Code** | Terraform modules for Azure deployment (Container Apps, Postgres, Key Vault) |
| **AI-Assisted Development** | `AGENTS.md` files throughout enable effective collaboration with coding agents |

---

## 🏗️ Architecture Overview

```
┌─────────────────────────────────────────────────────────────────────────┐
│                           React SPA (TanStack)                          │
│         TanStack Router · TanStack Query · shadcn/ui · Tailwind         │
└───────────────────────────────┬─────────────────────────────────────────┘
                                │ openapi-react-query (generated client)
                                ▼
┌─────────────────────────────────────────────────────────────────────────┐
│                      ASP.NET Core Minimal API                           │
│           OpenAPI 3.1 · FluentValidation · ASP.NET Identity             │
└───────────────────────────────┬─────────────────────────────────────────┘
                                │ EF Core
                                ▼
┌─────────────────────────────────────────────────────────────────────────┐
│                            PostgreSQL                                   │
└─────────────────────────────────────────────────────────────────────────┘
```

---

## 🔧 Tech Stack

### Backend (.NET 10)
- **ASP.NET Core Minimal APIs** with OpenAPI 3.1 documentation
- **EF Core** with PostgreSQL, code-first migrations, clean entity configuration
- **ASP.NET Identity** with [custom User](ProcureHub/Models/User.cs) and [Role](ProcureHub/Models/Role.cs) entities
- **FluentValidation** with automatic decorator-based validation ([ValidationRequestHandlerDecorator](ProcureHub/Infrastructure/ValidationRequestHandlerDecorator.cs))
- **Problem Details** for consistent error responses ([ToProblemDetails](ProcureHub.WebApi/Helpers/ResultExtensions.cs))

### Frontend (React + TypeScript)
- **TanStack Router** for type-safe routing
- **TanStack Query** via [openapi-react-query](https://openapi-ts.dev/openapi-react-query/) for data fetching
- **shadcn/ui** component library with Tailwind CSS
- **Feature-based structure** mirroring backend organization

---

## 📁 Project Structure

```
ProcureHub/                    # Core domain library
├── Features/                  # VSA feature folders (Users, Departments, PurchaseRequests, etc.)
├── Models/                    # EF Core entities with IEntityTypeConfiguration
├── Common/                    # Result<T>, Error, Pagination helpers
└── Infrastructure/            # IRequestHandler<> pattern, validation decorators

ProcureHub.WebApi/             # API host
├── Features/                  # Endpoint configuration per feature
├── Program.cs                 # App configuration, OpenAPI setup
└── Helpers/                   # Result-to-ProblemDetails mapping

ProcureHub.WebApi.Tests/       # Integration tests (xUnit v3)
└── Features/                  # Feature-specific test classes

ProcureHub.WebApp/             # React SPA
├── src/features/              # Feature modules (auth, users, purchase-requests, etc.)
├── src/routes/                # TanStack Router route definitions
└── src/lib/api/               # Generated API client
```

---

## 🎨 Notable Patterns & Techniques

### Type-Safe API Client Generation
The API exposes an OpenAPI spec that generates a fully typed TypeScript client:

```bash
npm run generate:api-schema  # Regenerates client from OpenAPI spec
```

The [API client](ProcureHub.WebApp/src/lib/api/client.ts) wraps TanStack Query, providing type-safe hooks with loading states, error handling, and caching out of the box.

### Transport-Agnostic Domain Logic
Request handlers return a [Result&lt;T&gt;](ProcureHub/Common/Result.cs) type, keeping domain logic decoupled from HTTP concerns:

```csharp
// Handler returns domain result
public async Task<Result<string>> HandleAsync(Request request, CancellationToken token)
{
    // ... domain logic
    return Result.Success(userId);
    // or: return Result.Failure<string>(UserErrors.EmailTaken);
}

// Endpoint maps to HTTP response
result.Match(
    userId => Results.Created($"/users/{userId}", new { userId }),
    error => error.ToProblemDetails()  // Converts Error → RFC 9457 Problem Details
);
```

### Comprehensive Endpoint Testing
All endpoints are tested for authentication, authorization, and validation using parameterized tests:

```csharp
public static TheoryData<EndpointInfo> GetAllUserEndpoints() => new()
{
    new EndpointInfo("/users", "POST", "CreateUser"),
    new EndpointInfo("/users", "GET", "QueryUsers"),
    // ... every endpoint listed
};

[Theory, MemberData(nameof(GetAllUserEndpoints))]
public async Task All_user_endpoints_require_authentication(EndpointInfo endpoint)
{
    var resp = await HttpClient.SendAsync(new HttpRequestMessage(new HttpMethod(endpoint.Method), endpoint.Path));
    Assert.Equal(HttpStatusCode.Unauthorized, resp.StatusCode);
}
```

See: [UserTests.cs](ProcureHub.WebApi.Tests/Features/UserTests.cs)

### OpenAPI Schema Customization
Custom logic ensures nested response types generate unique schema names ([CreateOpenApiSchemaReferenceId](ProcureHub.WebApi/Program.cs#L258)):

```csharp
// Transforms DataResponse<GetUserById.Response> → "DataResponseOfGetUserByIdResponse"
// instead of the default "DataResponseOfResponse" collision
```

### Identity API Fixes
The standard `MapIdentityApi` required two fixes for production use:
- Added missing 401 response documentation for `/login` endpoint
- Added `/logout` endpoint (not included by default)

See: [ConfigureIdentityApiEndpoints](ProcureHub.WebApi/Program.cs#L202)

---

## 🚀 Deployment

### Infrastructure
- **Terraform modules** in [`/infra`](infra/) for Azure resources:
  - Container Apps (API hosting)
  - Azure Database for PostgreSQL Flexible Server
  - Key Vault, Log Analytics, Application Insights
  - GitHub OIDC for secure CI/CD authentication

### CI/CD
- **GitHub Actions** workflows for build, test, and deploy
- API containerized with Docker, pushed to GitHub Container Registry
- Frontend deployed to Azure Static Web Apps

---

## 🏃 Running Locally

### Prerequisites
- .NET 10 SDK
- Node.js 20+
- Docker & Docker Compose

### Start databases
```bash
docker compose up -d
```

### Run API
```bash
cd ProcureHub.WebApi
dotnet run
```

### Run frontend
```bash
cd ProcureHub.WebApp
npm install
npm run dev
```

### Run tests
```bash
dotnet test ProcureHub.sln
```

---

## 🤖 AI-Assisted Development

This project is structured for effective collaboration with AI coding agents:

- **`AGENTS.md` files** at project root and in key directories provide context and conventions
- **Context documents** in `.context/` describe the domain and use cases
- **Consistent patterns** established manually first, then agents pointed to good examples

The approach: establish clear patterns → document them → let AI assist with the repetitive parts.

---

## 📋 Current Features

| Feature | Requester | Approver | Admin |
|---------|:---------:|:--------:|:-----:|
| Create Purchase Requests | ✓ | | |
| View Own Requests | ✓ | | |
| Approve/Reject Requests | | ✓ | |
| Manage Users | | | ✓ |
| Manage Departments | | | ✓ |
| Manage Categories | | | ✓ |

---

## 🔗 Key Files to Explore

- **API Endpoint Pattern**: [Users/Endpoints.cs](ProcureHub.WebApi/Features/Users/Endpoints.cs)
- **Request Handler Pattern**: [CreateUser.cs](ProcureHub/Features/Users/CreateUser.cs)
- **Integration Tests**: [UserTests.cs](ProcureHub.WebApi.Tests/Features/UserTests.cs)
- **Frontend Feature Module**: [purchase-requests/](ProcureHub.WebApp/src/features/purchase-requests)
- **Generated API Client**: [client.ts](ProcureHub.WebApp/src/lib/api/client.ts)


