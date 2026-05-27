import React, { createContext, useEffect, useMemo, useState } from "react";
import type { AuthResponse } from "../types";

type AuthUser = {
  name: string;
  email: string;
};

type AuthContextValue = {
  user: AuthUser | null;
  token: string | null;
  isAuthenticated: boolean;
  login: (auth: AuthResponse) => void;
  logout: () => void;
};

export const AuthContext = createContext<AuthContextValue | undefined>(undefined);

function decodeJwtPayload(token: string): Record<string, unknown> | null {
  const parts = token.split(".");
  if (parts.length !== 3) return null;

  const payload = parts[1] ?? "";
  const padded = payload.padEnd(payload.length + ((4 - (payload.length % 4)) % 4), "=");
  const base64 = padded.replace(/-/g, "+").replace(/_/g, "/");

  try {
    const json = atob(base64);
    return JSON.parse(json) as Record<string, unknown>;
  } catch {
    return null;
  }
}

export function AuthProvider({ children }: { children: React.ReactNode }) {
  const [token, setToken] = useState<string | null>(null);
  const [user, setUser] = useState<AuthUser | null>(null);

  const isAuthenticated = Boolean(token);

  const login = (auth: AuthResponse) => {
    localStorage.setItem("token", auth.token);
    setToken(auth.token);
    setUser({ email: auth.email, name: auth.name });

    // best-effort: validate token shape (no console output)
    decodeJwtPayload(auth.token);
  };

  const logout = () => {
    localStorage.removeItem("token");
    setToken(null);
    setUser(null);
    if (window.location.pathname !== "/login") {
      window.location.assign("/login");
    }
  };

  useEffect(() => {
    const existing = localStorage.getItem("token");
    if (!existing) return;

    const payload = decodeJwtPayload(existing);
    const email = typeof payload?.email === "string" ? payload.email : "";
    const name = typeof payload?.name === "string" ? payload.name : "";

    if (email && name) {
      setToken(existing);
      setUser({ email, name });
      return;
    }

    // fallback: keep token but user unknown (still authenticated)
    setToken(existing);
    setUser(null);
  }, []);

  const value = useMemo<AuthContextValue>(
    () => ({ user, token, isAuthenticated, login, logout }),
    [user, token, isAuthenticated]
  );

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>;
}

