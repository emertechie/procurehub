import { api } from "@/lib/api/client";
import { useQueryClient } from "@tanstack/react-query";

export function useDepartments() {
  return api.useQuery("get", "/departments");
}

export function useCreateDepartment() {
  const queryClient = useQueryClient();

  return api.useMutation("post", "/departments", {
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["get", "/departments"] });
    },
  });
}

export function useUpdateDepartment() {
  const queryClient = useQueryClient();

  return api.useMutation("put", "/departments/{id}", {
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["get", "/departments"] });
    },
  });
}

export function useDeleteDepartment() {
  const queryClient = useQueryClient();

  return api.useMutation("delete", "/departments/{id}", {
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["get", "/departments"] });
    },
  });
}
