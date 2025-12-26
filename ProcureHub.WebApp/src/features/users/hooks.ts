import { api } from "@/lib/api/client";
import { useQueryClient } from "@tanstack/react-query";

export function useUsers(email?: string, page = 1, pageSize = 20) {
  return api.useQuery("get", "/users", {
    params: {
      query: {
        Email: email || undefined,
        Page: page,
        PageSize: pageSize,
      },
    },
  });
}

export function useCreateUser() {
  const queryClient = useQueryClient();

  return api.useMutation("post", "/users", {
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["/users"] });
    },
  });
}

export function useUpdateUser() {
  const queryClient = useQueryClient();

  return api.useMutation("put", "/users/{id}", {
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["/users"] });
    },
  });
}

export function useEnableUser() {
  const queryClient = useQueryClient();

  return api.useMutation("patch", "/users/{id}/enable", {
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["/users"] });
    },
  });
}

export function useDisableUser() {
  const queryClient = useQueryClient();

  return api.useMutation("patch", "/users/{id}/disable", {
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["/users"] });
    },
  });
}

export function useAssignUserToDepartment() {
  const queryClient = useQueryClient();

  return api.useMutation("patch", "/users/{id}/department", {
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["/users"] });
    },
  });
}

export function useAssignRole() {
  const queryClient = useQueryClient();

  return api.useMutation("post", "/users/{userId}/roles", {
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["/users"] });
    },
  });
}

export function useRemoveRole() {
  const queryClient = useQueryClient();

  return api.useMutation("delete", "/users/{userId}/roles/{roleId}", {
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["/users"] });
    },
  });
}
