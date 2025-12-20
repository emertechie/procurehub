import { useContext } from "react";
import { redirect } from "@tanstack/react-router";
import { useQueryClient } from "@tanstack/react-query";
import { AuthContext } from "./provider";
import type { AuthContext as AuthContextValue } from "./types";
import { api } from "@/lib/api/client";

export function useAuth() {
  const ctx = useContext(AuthContext);
  if (!ctx) {
    throw new Error("useAuth must be used within AuthProvider");
  }
  return ctx;
}

export function ensureAuthenticated(
  authContext: AuthContextValue,
  currentHref: string,
) {
  if (!authContext.isAuthenticated) {
    throw redirect({
      to: "/login",
      search: {
        redirect: currentHref,
      },
    });
  }
}

const userQueryKey = ["get", "/me"];

export function useLoginMutation() {
  const queryClient = useQueryClient();
  return api.useMutation("post", "/login", {
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: userQueryKey });
    },
  });
}

export function useRegisterMutation() {
  const queryClient = useQueryClient();
  return api.useMutation("post", "/register", {
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: userQueryKey });
    },
  });
}

export function useLogoutMutation() {
  const queryClient = useQueryClient();
  return api.useMutation("post", "/logout", {
    onSuccess: () => {
      queryClient.setQueryData(userQueryKey, null);
    },
  });
}
