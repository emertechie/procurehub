import React, { createContext, useContext, useEffect, useState } from "react";

type User = {
  id: string;
  email: string;
};

export interface AuthContext {
  loading: boolean
  user: User | null
  isAuthenticated: boolean
  login: (email: string, password: string) => Promise<void>
  register: (email: string, password: string) => Promise<void>
  logout: () => Promise<void>
}

const AuthContext = createContext<AuthContext | undefined>(undefined);

export function AuthProvider({ children }: { children: React.ReactNode }) {
  const [user, setUser] = useState<User | null>(null);
  const [loading, setLoading] = useState(true);

  const isAuthenticated = !!user

  useEffect(() => {
    let cancelled = false;

    const loadUser = async () => {
      try {
        const res = await fetch("/api/me", {
          credentials: "include",
        });
        if (!res.ok) {
          throw new Error("Not authenticated");
        }
        const data = (await res.json()) as User;
        if (!cancelled) {
          setUser(data);
        }
      } catch (_err) {
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
    const res = await fetch("/api/login?useCookies=true", {
      method: "POST",
      headers: {
        "Content-Type": "application/json",
      },
      body: JSON.stringify({ email, password }),
      credentials: "include",
    });

    if (!res.ok) {
      let message = "Unable to login. Please check your details.";
      try {
        const data = await res.json();
        if (data?.message && typeof data.message === "string") {
          message = data.message;
        }
      } catch {
        // ignore json parse errors
      }
      throw new Error(message);
    }

    // Prefer user from response if present, otherwise fall back to /api/me
    const data = (await res.json().catch(() => null)) as { user?: User } | null;
    if (data?.user) {
      setUser({ id: data.user.id, email: data.user.email });
      return;
    }

    const meRes = await fetch("/api/me", { credentials: "include" });
    if (meRes.ok) {
      const me = (await meRes.json()) as User;
      setUser(me);
    }
  };

  const register = async (email: string, password: string) => {
    const res = await fetch("/api/register?useCookies=true", {
      method: "POST",
      headers: {
        "Content-Type": "application/json",
      },
      body: JSON.stringify({ email, password }),
      credentials: "include",
    });

    if (!res.ok) {
      let message = "Unable to register. Please check your details.";
      try {
        const data = await res.json();
        if (data?.message && typeof data.message === "string") {
          message = data.message;
        }
      } catch {
        // ignore json parse errors
      }
      throw new Error(message);
    }

    const data = (await res.json().catch(() => null)) as { user?: User } | null;
    if (data?.user) {
      setUser({ id: data.user.id, email: data.user.email });
      return;
    }

    const meRes = await fetch("/api/me", { credentials: "include" });
    if (meRes.ok) {
      const me = (await meRes.json()) as User;
      setUser(me);
    }
  };

  const logout = async () => {
    setUser(null);
    await fetch("/api/logout", {
      method: "POST",
      credentials: "include",
    }).catch(() => {
      // ignore logout errors
    });
  };

  return (
      <AuthContext.Provider value={{ loading, isAuthenticated, user, register, login, logout }}>
        {children}
      </AuthContext.Provider>
  )
}

export function useAuth() {
  const ctx = useContext(AuthContext);
  if (!ctx) {
    throw new Error("useAuth must be used within AuthProvider");
  }
  return ctx;
}
