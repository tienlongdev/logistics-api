"use client";

import { create } from "zustand";
import { createJSONStorage, persist } from "zustand/middleware";

import { decodeAccessToken } from "@/lib/auth/jwt";
import type { ApiRole, LoginResponse } from "@/lib/types/api";

type AuthMode = "api";

interface AuthState {
  accessToken: string | null;
  refreshToken: string | null;
  expiresAt: number | null;
  email: string | null;
  roles: ApiRole[];
  mode: AuthMode | null;
  hydrated: boolean;
  isRestoring: boolean;
  setSession: (payload: {
    accessToken: string;
    refreshToken: string;
    expiresIn: number;
    email: string;
    mode: AuthMode;
  }) => void;
  updateTokens: (payload: LoginResponse) => void;
  clearSession: () => void;
  startRestoring: () => void;
  finishRestoring: () => void;
  markHydrated: () => void;
}

export const useAuthStore = create<AuthState>()(
  persist(
    (set) => ({
      accessToken: null,
      refreshToken: null,
      expiresAt: null,
      email: null,
      roles: [],
      mode: null,
      hydrated: false,
      isRestoring: false,
      setSession: ({ accessToken, refreshToken, expiresIn, email, mode }) =>
        set({
          accessToken,
          refreshToken,
          expiresAt: Date.now() + expiresIn * 1000,
          email,
          roles: decodeAccessToken(accessToken).roles,
          mode,
        }),
      updateTokens: ({ accessToken, refreshToken, expiresIn }) =>
        set((state) => ({
          accessToken,
          refreshToken,
          expiresAt: Date.now() + expiresIn * 1000,
          roles: decodeAccessToken(accessToken).roles,
          mode: state.mode ?? "api",
        })),
      clearSession: () =>
        set({
          accessToken: null,
          refreshToken: null,
          expiresAt: null,
          email: null,
          roles: [],
          mode: null,
          isRestoring: false,
        }),
      startRestoring: () => set({ isRestoring: true }),
      finishRestoring: () => set({ isRestoring: false }),
      markHydrated: () => set({ hydrated: true }),
    }),
    {
      name: "logistics-web-auth",
      storage: createJSONStorage(() => sessionStorage),
      partialize: (state) => ({
        email: state.email,
        mode: state.mode,
        refreshToken: state.refreshToken,
      }),
      onRehydrateStorage: () => (state) => {
        state?.markHydrated();
      },
    },
  ),
);