# General

- Prefer to use TanStack Query instead of raw fetch calls
- After changes, run `npm run typecheck` to ensure no new type errors introduced
- Always run prettier to format new or updated files

# React

- When using JSX, always add a `import * as React from "react";` line at top of file
- Favor small, single-responsibility components (one clear purpose, ideally <150 lines); break down large UIs into multiple nested components instead of one monolithic file.
- When a component grows beyond a clear responsibility, extract subcomponents and compose them rather than extending the file.

# API calls

- Always use the client in `ProcureHub.WebApp/src/lib/api/client.ts` to make API calls.
  - The client is a strongly typed `openapi-react-query` wrapper around TanStack Query (formerly known as React Query).
  - Use API client return values like `isPending`, `isError`, `data`, `error` etc to cleanly implement React components.
  - The client is generated from the OpenAPI exposed by the `ProcureHub.WebApi` project using `npm run generate:api-schema`.

# Project structure

- Project structure and feature layout rules are defined in: `docs/project-structure.md`

# Shadcn instructions

- Never create Shadcn components manually. Always install the latest version using command like below:

```bash
npx shadcn@latest add button
```
