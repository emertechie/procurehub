import { api } from "@/lib/api/client";
import { useQueryClient } from "@tanstack/react-query";
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

export function usePurchaseRequest(id: string) {
  return api.useQuery("get", "/purchase-requests/{id}", {
    params: { path: { id } },
  });
}

export function useCreatePurchaseRequest() {
  const queryClient = useQueryClient();

  return api.useMutation("post", "/purchase-requests", {
    onSuccess: () => {
      queryClient.invalidateQueries({
        queryKey: ["get", "/purchase-requests"],
      });
    },
  });
}

export function useUpdatePurchaseRequest() {
  const queryClient = useQueryClient();

  return api.useMutation("put", "/purchase-requests/{id}", {
    onSuccess: (_data, variables) => {
      queryClient.invalidateQueries({
        queryKey: ["get", "/purchase-requests"],
      });
      queryClient.invalidateQueries({
        queryKey: [
          "get",
          "/purchase-requests/{id}",
          { id: variables.params.path.id },
        ],
      });
    },
  });
}

export function useSubmitPurchaseRequest() {
  const queryClient = useQueryClient();

  return api.useMutation("post", "/purchase-requests/{id}/submit", {
    onSuccess: (_data, variables) => {
      queryClient.invalidateQueries({
        queryKey: ["get", "/purchase-requests"],
      });
      queryClient.invalidateQueries({
        queryKey: [
          "get",
          "/purchase-requests/{id}",
          { id: variables.params.path.id },
        ],
      });
    },
  });
}

export function useDeletePurchaseRequest() {
  const queryClient = useQueryClient();

  return api.useMutation("delete", "/purchase-requests/{id}", {
    onSuccess: () => {
      queryClient.invalidateQueries({
        queryKey: ["get", "/purchase-requests"],
      });
    },
  });
}
