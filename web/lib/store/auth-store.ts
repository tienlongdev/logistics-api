"use client";

import { create } from "zustand";
import { persist } from "zustand/middleware";

type AuthMode = "api" | "demo";

interface AuthState {
  accessToken: string | null;
  refreshToken: string | null;
  expiresAt: number | null;
  email: string | null;
  mode: AuthMode | null;
  hydrated: boolean;
  setSession: (payload: {
    accessToken: string;
    refreshToken: string;
    expiresIn: number;
    email: string;
    mode: AuthMode;
  }) => void;
  clearSession: () => void;
  markHydrated: () => void;
}

export const useAuthStore = create<AuthState>()(
  persist(
    (set) => ({
      accessToken: null,
      refreshToken: null,
      expiresAt: null,
      email: null,
      mode: null,
      hydrated: false,
      setSession: ({ accessToken, refreshToken, expiresIn, email, mode }) =>
        set({
          accessToken,
          refreshToken,
          expiresAt: Date.now() + expiresIn * 1000,
          email,
          mode,
        }),
      clearSession: () =>
        set({
          accessToken: null,
          refreshToken: null,
          expiresAt: null,
          email: null,
          mode: null,
        }),
      markHydrated: () => set({ hydrated: true }),
    }),
    {
      name: "logistics-web-auth",
      onRehydrateStorage: () => (state) => {
        state?.markHydrated();
      },
    },
  ),
);