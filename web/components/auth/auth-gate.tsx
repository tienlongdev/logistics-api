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
            <CardTitle>Đang khôi phục phiên đăng nhập</CardTitle>
            <CardDescription>
              Vui lòng chờ trong giây lát...
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
            <CardTitle>Yêu cầu đăng nhập</CardTitle>
            <CardDescription>
              Bạn cần đăng nhập để xem trang này.
            </CardDescription>
          </CardHeader>
          <CardContent>
            <Button asChild>
              <Link href="/login">Đăng nhập</Link>
            </Button>
          </CardContent>
        </Card>
      </div>
    );
  }

  return <>{children}</>;
}