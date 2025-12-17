import React, { createContext, useContext, useEffect, useState } from "react";

type User = {
  id: string;
  email: string;
};

type AuthContextValue = {
  user: User | null;
  loading: boolean;
  login: (email: string, password: string) => Promise<void>;
  register: (email: string, password: string) => Promise<void>;
  logout: () => void;
};

const AuthContext = createContext<AuthContextValue | undefined>(undefined);

export function AuthProvider({ children }: { children: React.ReactNode }) {
  const [user, setUser] = useState<User | null>(null);
  const [loading, setLoading] = useState(true);

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

  const logout = () => {
    setUser(null);
    void fetch("/api/logout", {
      method: "POST",
      credentials: "include",
    }).catch(() => {
      // ignore logout errors
    });
  };

  const value: AuthContextValue = {
    user,
    loading,
    login,
    register,
    logout,
  };

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>;
}

export function useAuth() {
  const ctx = useContext(AuthContext);
  if (!ctx) {
    throw new Error("useAuth must be used within AuthProvider");
  }
  return ctx;
}
