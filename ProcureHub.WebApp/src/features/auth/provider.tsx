import React from 'react';
import { createContext, useEffect, useState, type ReactNode } from "react";

import {
  fetchCurrentUser,
  loginWithCredentials,
  logoutFromSession,
  registerWithCredentials,
} from "./api";
import type { AuthContext as AuthContextValue, AuthUser } from "./types";

export const AuthContext = createContext<AuthContextValue | undefined>(
  undefined,
);

export function AuthProvider({ children }: { children: ReactNode }) {
  const [user, setUser] = useState<AuthUser | null>(null);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    let cancelled = false;

    const loadUser = async () => {
      try {
        const data = await fetchCurrentUser();
        if (!cancelled) {
          setUser(data);
        }
      } catch {
        if (!cancelled) {
          setUser(null);
        }
      } finally {
        if (!cancelled) {
          setLoading(false);
        }
      }
    };

    loadUser();

    return () => {
      cancelled = true;
    };
  }, []);

  const login = async (email: string, password: string) => {
    const nextUser = await loginWithCredentials(email, password);
    if (nextUser) {
      setUser(nextUser);
    }
  };

  const register = async (email: string, password: string) => {
    const nextUser = await registerWithCredentials(email, password);
    if (nextUser) {
      setUser(nextUser);
    }
  };

  const logout = async () => {
    setUser(null);
    await logoutFromSession();
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
