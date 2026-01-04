# ProcureHub

## Overview

ProcureHub is an **internal procurement and purchase approval system** designed to model a realistic enterprise workflow. It allows staff to create purchase requests and routes them through policy-driven approval rules based on amount, category, department, and user role.

The system is intentionally scoped to demonstrate:
- clear domain modelling
- explicit business rules
- workflow/state management
- role- and policy-based authorisation

---

## Core Use Cases

### Purchase Request Lifecycle

1. A user creates a **purchase request** in Draft state
2. The request is edited while in Draft
3. The request is **submitted** for approval
4. One or more authorised users **approve or reject** the request
5. The request reaches a final state (Approved or Rejected)

### User Interactions

- Requestors can create and submit requests
- Approvers can approve or reject submitted requests
- All decisions are recorded for auditability

---

## User Types & Roles

### Requestor
- Creates purchase requests
- Edits requests while in Draft
- Submits requests for approval
- Views their own requests and outcomes

### Approver (Department Manager)
- Reviews submitted requests from their department
- Approves or rejects requests they are authorised for
- Cannot approve their own requests

### Finance Approver
- Reviews high-value or policy-restricted requests
- Can approve requests across all departments
- Cannot approve their own requests

### Administrator
- Manages users and role assignments
- Has read access to all requests
- Does not participate in approvals by default

---

## Approval Policy Rules (Examples)

Approval requirements are determined by policy rules evaluated when a request is submitted.

Example rules:

- Requests **≤ €1,000** are auto-approved
- Requests **€1,000 – €10,000** require department approval
- Requests **> €10,000** require finance approval
- Certain categories (e.g. IT equipment) always require finance approval
- A requestor can never approve their own request

Policies are implemented in code and can be extended later.

---

## Purchase Request States

### States

- `Draft`
- `Submitted`
- `Approved`
- `Rejected`

### Allowed Transitions

- `Draft → Submitted`
- `Submitted → Approved`
- `Submitted → Rejected`

All other transitions are invalid.

### State Semantics

- **Draft**: Editable, not visible to approvers
- **Submitted**: Frozen, awaiting approval decisions
- **Approved**: Final, immutable
- **Rejected**: Final, immutable

---

## Domain Model Summary

### Aggregate Root

**PurchaseRequest**
- Owns the request lifecycle and state
- Enforces valid state transitions
- Records approval decisions
- Prevents invalid mutations

### Internal Entity

**Approval**
- Represents an individual approval or rejection
- Exists only within a PurchaseRequest

### Supporting Entity

**User**
- Represents an authenticated system user
- Includes role and department information

---

## Architectural Notes

- Business rules live in the domain layer
- State changes occur only through explicit domain methods
- APIs map HTTP requests to application use cases
- Persistence uses EF Core with encapsulated domain models

---

## Out of Scope (Initial Version)

- Budgets and cost centres
- Vendors and purchase orders
- Purchase orders
- Time-based escalation or SLAs
- Notifications and messaging
- Configurable policies via UI

These may be added in future iterations.

---

## Natural Future Extensions

- Delegated approvals (out-of-office)
- Budget checks
- Attachment uploads (quotes)
- Read-only reporting dashboards
- Configurable approval policies via admin UI
- Domain events and audit log viewer

Each extension builds on the same core domain without changing fundamentals.

----

## Intent

This project aims to be **small, understandable, and realistic**, while still demonstrating how enterprise systems model workflows, enforce policy rules, and protect domain integrity.
