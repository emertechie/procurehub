- In all interactions, plans, and commit messages, be extremely concise and sacrifice grammar for the sake of concision.
- Prefer to use TanStack Query instead of raw fetch calls, when it makes sense

# API calls

- Always use the client in `ProcureHub.WebApp/src/lib/api/client.ts` to make API calls.
  - The client is a type-safe wrapper around TanStack Query (formerly known as React Query).
  - Use API client return values like `isPending`, `isError`, `data`, `error` etc to cleanly implement React components.
  - The client is generated from the OpenAPI exposed by the `ProcureHub.WebApi` project using `npm run generate:api-schema`.

# Project structure

- Project structure and feature layout rules are defined in: docs/project-structure.md

# Shadcn instructions

- Never create Shadcn components manually. Always install the latest version using command like below:

```bash
npx shadcn@latest add button
```
