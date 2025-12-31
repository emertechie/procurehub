import { api } from "@/lib/api/client";
import type { PurchaseRequestStatusValue } from "./types";

export function usePurchaseRequests(
  status?: PurchaseRequestStatusValue,
  search?: string,
  page = 1,
  pageSize = 20,
) {
  return api.useQuery("get", "/purchase-requests", {
    params: {
      query: {
        status: status,
        search: search || undefined,
        page: page,
        pageSize: pageSize,
      },
    },
  });
}
