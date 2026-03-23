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
  const { accessToken, hydrated } = useAuthStore((state) => ({
    accessToken: state.accessToken,
    hydrated: state.hydrated,
  }));

  if (!hydrated) {
    return null;
  }

  if (pathname === "/login") {
    return <>{children}</>;
  }

  if (!accessToken) {
    return (
      <div className="page-shell flex min-h-screen items-center justify-center">
        <Card className="max-w-xl">
          <CardHeader>
            <div className="mb-4 flex h-12 w-12 items-center justify-center rounded-2xl bg-accent text-accent-foreground">
              <LockKeyhole className="h-6 w-6" />
            </div>
            <CardTitle>Auth gate placeholder</CardTitle>
            <CardDescription>
              Route nay can session dang nhap. Dang su dung auth gate placeholder de khoa khu vuc van hanh trong luc backend auth dang duoc ket noi.
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