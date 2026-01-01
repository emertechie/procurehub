import { api } from "@/lib/api/client";
import { useQueryClient } from "@tanstack/react-query";
import { useNavigate } from "@tanstack/react-router";
import { toast } from "sonner";
import type {
  PurchaseRequestStatusValue,
  CreatePurchaseRequestFormData,
  UpdatePurchaseRequestFormData,
} from "./types";

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

/**
 * Composite hook for editing a purchase request.
 * Encapsulates update, submit, and delete mutations with navigation and toast feedback.
 */
export function useEditPurchaseRequest(id: string) {
  const navigate = useNavigate();
  const updateMutation = useUpdatePurchaseRequest();
  const submitMutation = useSubmitPurchaseRequest();
  const deleteMutation = useDeletePurchaseRequest();

  const updateRequest = async (
    data: CreatePurchaseRequestFormData | UpdatePurchaseRequestFormData,
  ) => {
    return updateMutation.mutateAsync({
      params: { path: { id } },
      body: {
        id,
        title: data.title,
        description: data.description || null,
        estimatedAmount: data.estimatedAmount,
        businessJustification: data.businessJustification || null,
        categoryId: data.categoryId,
        departmentId: data.departmentId,
      },
    });
  };

  const handleSaveAsDraft = async (
    data: CreatePurchaseRequestFormData | UpdatePurchaseRequestFormData,
  ) => {
    try {
      await updateRequest(data);
      toast.success("Purchase request updated");
      navigate({ to: "/requests" });
    } catch {
      // Error handled via form
    }
  };

  const handleSubmitForApproval = async (
    data: CreatePurchaseRequestFormData | UpdatePurchaseRequestFormData,
  ) => {
    try {
      await updateRequest(data);
      await submitMutation.mutateAsync({
        params: { path: { id } },
      });
      toast.success("Purchase request submitted for approval");
      navigate({ to: "/requests" });
    } catch {
      // Error handled via form
    }
  };

  const handleDelete = async () => {
    try {
      await deleteMutation.mutateAsync({
        params: { path: { id } },
      });
      toast.success("Purchase request deleted");
      navigate({ to: "/requests" });
    } catch {
      toast.error("Failed to delete purchase request");
    }
  };

  return {
    handleSaveAsDraft,
    handleSubmitForApproval,
    handleDelete,
    isSaving: updateMutation.isPending && !submitMutation.isPending,
    isSubmitting: submitMutation.isPending,
    isDeleting: deleteMutation.isPending,
    saveError: updateMutation.error,
    submitError: submitMutation.error || updateMutation.error,
  };
}
