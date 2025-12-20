import React from "react";
import { createContext, type ReactNode } from "react";
import { useQueryClient } from "@tanstack/react-query";
import { api } from "@/lib/procurehub-api/api-client";
import type { AuthContext as AuthContextValue } from "./types";

export const AuthContext = createContext<AuthContextValue | undefined>(
  undefined,
);

export function AuthProvider({ children }: { children: ReactNode }) {
  const queryClient = useQueryClient();

  // Current user query
  const { data: user = null, isLoading: loading } = api.useQuery(
    "get",
    "/me",
    undefined,
    {
      retry: false,
    },
  );

  const currentUserQueryKey = ["get", "/me"];

  const loginMutation = api.useMutation("post", "/login", {
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: currentUserQueryKey });
    },
  });

  const registerMutation = api.useMutation("post", "/register", {
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: currentUserQueryKey });
    },
  });

  const logoutMutation = api.useMutation("post", "/logout", {
    onSuccess: () => {
      queryClient.setQueryData(currentUserQueryKey, null);
    },
  });

  const login = async (email: string, password: string) => {
    await loginMutation.mutateAsync({
      params: {
        query: { useCookies: true },
      },
      body: { email, password },
    });
  };

  const register = async (email: string, password: string) => {
    await registerMutation.mutateAsync({
      body: { email, password },
    });
  };

  const logout = async () => {
    await logoutMutation.mutateAsync({});
  };

  const isAuthenticated = !!user;

  return (
    <AuthContext.Provider
      value={{ loading, isAuthenticated, user, register, login, logout }}
    >
      {children}
    </AuthContext.Provider>
  );
}
