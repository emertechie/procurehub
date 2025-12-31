import type { components } from "@/lib/api/schema";

export type PurchaseRequest =
  components["schemas"]["QueryPurchaseRequestsResponse"];

export type PurchaseRequestStatusValue =
  components["schemas"]["PurchaseRequestStatus"];

export const PurchaseRequestStatus = {
  Draft: "Draft",
  Pending: "Pending",
  Approved: "Approved",
  Rejected: "Rejected",
} as const satisfies Record<string, PurchaseRequestStatusValue>;
