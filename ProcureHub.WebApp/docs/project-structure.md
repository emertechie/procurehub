Use these rules when adding or generating new functionality.

## Canonical Directory Structure

```
src/
├─ routes/                # Navigation + route composition only
│  ├─ __root.tsx          # App root (providers, query client, auth context)
│  │
│  ├─ (public)/           # Unauthenticated pages
│  │  ├─ _layout.tsx
│  │  ├─ index.tsx        # Landing page
│  │  ├─ login.tsx
│  │  └─ register.tsx
│  │
│  ├─ (auth)/             # Authenticated app area
│  │  ├─ _layout.tsx      # Auth guard (redirect if not logged in)
│  │  ├─ index.tsx        # Dashboard / app home
│  │  ├─ settings.tsx
│  │  └─ admin/
│  │     └─ users.tsx
│  │
│  └─ _404.tsx
│
├─ features/              # Business capabilities (domain logic)
│  ├─ auth/
│  │  ├─ api.ts           # login, logout, refresh
│  │  ├─ hooks.ts         # useAuth, useCurrentUser
│  │  ├─ store.ts         # auth state (Zustand, etc.)
│  │  └─ types.ts
│  │
│  ├─ users/
│  │  ├─ api.ts
│  │  ├─ hooks.ts
│  │  └─ types.ts
│
├─ components/            # Reusable UI (Button, Modal, FormField)
├─ lib/                   # Cross-cutting infra (api client, auth helpers)
└─ main.tsx
```

## Core Rules

1. **Routes are for navigation only** — no business logic, no API calls.

2. **Every business capability lives in `features/`** (auth, users, billing, etc.).

3. **Deleting a feature folder should remove a real app capability.**

4. **Public pages go in `(public)`; authenticated pages go in `(auth)`.**

5. **Auth is enforced once** in `(auth)/_layout.tsx` using `beforeLoad`.

6. **Routes call feature hooks/APIs; features never import routes.**

7. **Use nested layouts** for app chrome (sidebar, navbar), not per-page wrappers.

8. **UI primitives go in `components/`** (no domain or routing knowledge).

9. **Cross-cutting helpers go in `lib/`** (API client, query client, auth utils).

10. **Group by domain, not by file type.**

---

## Mental Model

- Routes decide **when** something runs
- Features decide **how** it works

Never mix the two.
