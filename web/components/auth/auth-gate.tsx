"use client";

import { LockKeyhole } from "lucide-react";
import Link from "next/link";
import { usePathname } from "next/navigation";
import { type ReactNode } from "react";

import { Button } from "@/components/ui/button";
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "@/components/ui/card";
import { useAuthStore } from "@/lib/store/auth-store";

interface AuthGateProps {
  children: ReactNode;
}

export function AuthGate({ children }: AuthGateProps) {
  const pathname = usePathname();
  const accessToken = useAuthStore((state) => state.accessToken);
  const hydrated = useAuthStore((state) => state.hydrated);
  const isRestoring = useAuthStore((state) => state.isRestoring);
  const refreshToken = useAuthStore((state) => state.refreshToken);

  const isPublicRoute = pathname === "/login" || pathname === "/tracking";

  if (!hydrated) {
    return null;
  }

  if (isPublicRoute) {
    return <>{children}</>;
  }

  if (isRestoring || (!accessToken && refreshToken)) {
    return (
      <div className="page-shell flex min-h-screen items-center justify-center">
        <Card className="max-w-xl">
          <CardHeader>
            <div className="mb-4 flex h-12 w-12 items-center justify-center rounded-2xl bg-accent text-accent-foreground">
              <LockKeyhole className="h-6 w-6" />
            </div>
            <CardTitle>Restoring session</CardTitle>
            <CardDescription>
              Access token chi duoc giu trong memory. Frontend dang dung refresh token tam thoi trong session storage de khoi phuc phien dang nhap.
            </CardDescription>
          </CardHeader>
        </Card>
      </div>
    );
  }

  if (!accessToken) {
    return (
      <div className="page-shell flex min-h-screen items-center justify-center">
        <Card className="max-w-xl">
          <CardHeader>
            <div className="mb-4 flex h-12 w-12 items-center justify-center rounded-2xl bg-accent text-accent-foreground">
              <LockKeyhole className="h-6 w-6" />
            </div>
            <CardTitle>Authentication required</CardTitle>
            <CardDescription>
              Route nay goi endpoint can bearer token. Dang nhap de frontend co the su dung `POST /api/v1/auth/login` va tu dong refresh khi gap `401`.
            </CardDescription>
          </CardHeader>
          <CardContent>
            <Button asChild>
              <Link href="/login">Di den trang dang nhap</Link>
            </Button>
          </CardContent>
        </Card>
      </div>
    );
  }

  return <>{children}</>;
}