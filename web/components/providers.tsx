"use client";

import { QueryClientProvider } from "@tanstack/react-query";
import { ThemeProvider } from "next-themes";
import { type ReactNode, useEffect, useState } from "react";
import { Toaster } from "sonner";

import { restoreSession } from "@/lib/api/client";
import { createQueryClient } from "@/lib/query/client";
import { useAuthStore } from "@/lib/store/auth-store";

interface ProvidersProps {
  children: ReactNode;
}

function AuthSessionBootstrap() {
  const accessToken = useAuthStore((state) => state.accessToken);
  const finishRestoring = useAuthStore((state) => state.finishRestoring);
  const hydrated = useAuthStore((state) => state.hydrated);
  const isRestoring = useAuthStore((state) => state.isRestoring);
  const refreshToken = useAuthStore((state) => state.refreshToken);
  const startRestoring = useAuthStore((state) => state.startRestoring);

  useEffect(() => {
    if (!hydrated || accessToken || !refreshToken || isRestoring) {
      return;
    }

    startRestoring();

    void restoreSession().finally(() => {
      finishRestoring();
    });
  }, [accessToken, finishRestoring, hydrated, isRestoring, refreshToken, startRestoring]);

  return null;
}

export function Providers({ children }: ProvidersProps) {
  const [queryClient] = useState(() => createQueryClient());

  return (
    <ThemeProvider attribute="class" defaultTheme="light" enableSystem disableTransitionOnChange>
      <QueryClientProvider client={queryClient}>
        <AuthSessionBootstrap />
        {children}
        <Toaster
          richColors
          closeButton
          position="top-right"
          toastOptions={{
            classNames: {
              toast: "rounded-2xl border border-border/80 shadow-xl",
              title: "text-sm font-semibold",
              description: "text-sm",
            },
          }}
        />
      </QueryClientProvider>
    </ThemeProvider>
  );
}