import * as z from "zod";
import type { components } from "@/lib/api/schema";

export type PurchaseRequest =
  components["schemas"]["QueryPurchaseRequestsResponse"];

export type PurchaseRequestDetail =
  components["schemas"]["GetPurchaseRequestByIdResponse"];

export type PurchaseRequestStatusValue =
  components["schemas"]["PurchaseRequestStatus"];

export const PurchaseRequestStatus = {
  Draft: "Draft",
  Pending: "Pending",
  Approved: "Approved",
  Rejected: "Rejected",
} as const satisfies Record<string, PurchaseRequestStatusValue>;

export type Category = components["schemas"]["QueryCategoriesResponse"];
export type Department = components["schemas"]["QueryDepartmentsResponse"];

export const createPurchaseRequestSchema = z.object({
  title: z
    .string()
    .min(1, "Title is required")
    .max(200, "Title must be at most 200 characters"),
  description: z
    .string()
    .max(2000, "Description must be at most 2000 characters")
    .optional(),
  estimatedAmount: z.coerce.number().min(0, "Amount must be positive"),
  businessJustification: z
    .string()
    .max(1000, "Business justification must be at most 1000 characters")
    .optional(),
  categoryId: z.string().min(1, "Category is required"),
  departmentId: z.string().min(1, "Department is required"),
});

export const updatePurchaseRequestSchema = createPurchaseRequestSchema.extend({
  id: z.string(),
});

export type CreatePurchaseRequestFormData = z.infer<
  typeof createPurchaseRequestSchema
>;
export type UpdatePurchaseRequestFormData = z.infer<
  typeof updatePurchaseRequestSchema
>;
