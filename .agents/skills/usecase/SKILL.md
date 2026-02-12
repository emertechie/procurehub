---
name: usecase
description: How to implement a use case in this ProcureHub project. Use this whenever the user asks you to implement a use case.
---

Follow the steps below to implement a use case. Assume the UI will be implemented in the `ProcureHub.BlazorApp` Blazor project, unless told otherwise.

# 1. Create plan

## 1.1 Gather context

Read the following files to get context for the whole project:
- `.context/procure_hub_project_overview.md`
- `.context/all_use_cases.md`

Read the architecture and coding guidance files:
- `AGENTS.md` (root — architecture, code style, testing patterns, common commands)
- `ProcureHub/AGENTS.md` (domain models, migrations, VSA handlers)
- `ProcureHub.WebApi/Features/AGENTS.md` (endpoint mapping)
- `ProcureHub.BlazorApp/AGENTS.md` (if UI is in Blazor)

Find and read an existing, similar feature in the codebase as a reference implementation (e.g. look at the feature folders under `ProcureHub/Features/`, `ProcureHub.WebApi/Features/`, and the corresponding UI and tests).

## 1.2 Plan the use case

- With all the context in mind, plan the use case.
- If you are unsure about anything, ask the user for clarification before proceeding.

## 1.3 Write the plan to file

- Write the plan to a Markdown file in the `.plans/use-cases` folder, with a 2-digit incrementing prefix. Example: `01_create_new_product.md`.
- If appropriate, include a mermaid diagram in the plan to help explain more complex parts.
- Make sure to include a section at the end about what tests to write (unit and/or E2E).

## 1.4 Get approval

- IMPORTANT: Pause and get user approval of the plan before continuing.

# 2. Create steps

## 2.1 Break plan down into steps

- Break the plan down into steps that an AI agent can implement and check off one by one
- Break up into numbered phases if necessary.
- If any Blazor E2E tests are planned, make sure to add those as one of the last steps / phases. (So that the user can verify the functionality before investing in E2E tests, as the UI may change on review). 

## 2.2 Create steps file

- Write the steps to the same `.plans/use-cases` folder with the same filename, but with a `_steps` suffix. Example: `01_create_new_product_steps.md`.
- Include an empty checkbox for each step.  

## 2.3 Get approval

- IMPORTANT: Pause and get user approval of the steps before continuing.
- Remind the user that they may want to switch to a more affordable agent model before implementing.

# 3. Implement steps

Use the `steps` skill to implement the steps.