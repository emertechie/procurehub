import React from "react";
import { createContext, type ReactNode } from "react";
import { api } from "@/lib/api/client";
import type { AuthContext as AuthContextValue } from "./types";

export const AuthContext = createContext<AuthContextValue | undefined>(
  undefined,
);

export function AuthProvider({ children }: { children: ReactNode }) {
  // Current user query
  const { data: user = null, isLoading: loading } = api.useQuery(
    "get",
    "/me",
    undefined,
    {
      retry: false,
    },
  );

  const isAuthenticated = !!user;

  return (
    <AuthContext.Provider value={{ loading, isAuthenticated, user }}>
      {children}
    </AuthContext.Provider>
  );
}
