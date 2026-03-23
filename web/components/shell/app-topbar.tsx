"use client";

import { useMutation } from "@tanstack/react-query";
import { Menu, SearchSlash } from "lucide-react";
import Link from "next/link";
import { usePathname, useRouter } from "next/navigation";

import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
import { ThemeToggle } from "@/components/ui/theme-toggle";
import { logout } from "@/lib/api/client";
import { routeTitles } from "@/lib/constants/navigation";
import { useAuthStore } from "@/lib/store/auth-store";

interface AppTopbarProps {
  onOpenSidebar: () => void;
}

export function AppTopbar({ onOpenSidebar }: AppTopbarProps) {
  const pathname = usePathname();
  const router = useRouter();
  const { accessToken, email, mode, roles } = useAuthStore((state) => ({
    accessToken: state.accessToken,
    email: state.email,
    mode: state.mode,
    roles: state.roles,
  }));
  const logoutMutation = useMutation({
    mutationFn: logout,
    onSettled: () => {
      router.push("/login");
    },
  });

  const title = pathname.startsWith("/shipments/") ? "Shipment detail" : routeTitles[pathname] ?? "Operations";

  return (
    <header className="sticky top-0 z-30 border-b border-border/70 bg-background/80 backdrop-blur">
      <div className="page-shell flex items-center gap-3 py-4">
        <Button variant="outline" size="icon" className="lg:hidden" onClick={onOpenSidebar} aria-label="Open navigation menu">
          <Menu className="h-4 w-4" />
        </Button>
        <div className="min-w-0 flex-1">
          <div className="flex flex-wrap items-center gap-2">
            <h1 className="truncate text-xl font-semibold">{title}</h1>
            {mode ? <Badge variant="success">API auth</Badge> : null}
            {roles.map((role) => (
              <Badge key={role} variant="outline">{role}</Badge>
            ))}
          </div>
          <p className="text-sm text-muted-foreground">Typed client, correlation id header, va fallback ro rang cho shipment APIs chua co.</p>
        </div>
        <div className="hidden items-center gap-3 md:flex">
          <div className="flex items-center gap-2 rounded-full border border-border bg-card px-3 py-2 text-sm text-muted-foreground">
            <SearchSlash className="h-4 w-4" />
            <span className="max-w-40 truncate">{email ?? "No session"}</span>
          </div>
          <ThemeToggle />
          {accessToken ? (
            <Button variant="outline" disabled={logoutMutation.isPending} onClick={() => logoutMutation.mutate()}>
              Dang xuat
            </Button>
          ) : (
            <Button asChild variant="outline">
              <Link href="/login">Dang nhap</Link>
            </Button>
          )}
        </div>
      </div>
    </header>
  );
}