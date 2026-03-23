"use client";

import { useMutation } from "@tanstack/react-query";
import { LogOut, Menu, Search, SearchSlash } from "lucide-react";
import Link from "next/link";
import { usePathname, useRouter } from "next/navigation";
import { useEffect, useState } from "react";
import { toast } from "sonner";

import { ConfirmDialog } from "@/components/feedback/confirm-dialog";
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
  const [logoutDialogOpen, setLogoutDialogOpen] = useState(false);
  const accessToken = useAuthStore((state) => state.accessToken);
  const email = useAuthStore((state) => state.email);
  const mode = useAuthStore((state) => state.mode);
  const roles = useAuthStore((state) => state.roles);
  const logoutMutation = useMutation({
    mutationFn: logout,
    onSuccess: () => {
      toast.success("Da dang xuat khoi workspace");
    },
    onSettled: () => {
      setLogoutDialogOpen(false);
      router.push("/login");
    },
  });

  const title = pathname.startsWith("/shipments/") ? "Shipment detail" : routeTitles[pathname] ?? "Operations";

  useEffect(() => {
    const handleKeyDown = (event: KeyboardEvent) => {
      const isShortcut = (event.metaKey || event.ctrlKey) && event.key.toLowerCase() === "k";

      if (!isShortcut) {
        return;
      }

      event.preventDefault();
      router.push("/search?focus=primary");
    };

    window.addEventListener("keydown", handleKeyDown);
    return () => window.removeEventListener("keydown", handleKeyDown);
  }, [router]);

  return (
    <>
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
          <p className="text-sm text-muted-foreground">Typed client, correlation id headers, and graceful fallbacks for backend gaps.</p>
        </div>
        <div className="hidden items-center gap-3 md:flex">
          <Button variant="outline" onClick={() => router.push("/search?focus=primary")}>
            <Search className="h-4 w-4" />
            Tim nhanh
            <span className="keyboard-hint">{typeof navigator !== "undefined" && navigator.platform.includes("Mac") ? "⌘K" : "Ctrl+K"}</span>
          </Button>
          <div className="flex items-center gap-2 rounded-full border border-border bg-card px-3 py-2 text-sm text-muted-foreground">
            <SearchSlash className="h-4 w-4" />
            <span className="max-w-40 truncate">{email ?? "No session"}</span>
          </div>
          <ThemeToggle />
          {accessToken ? (
            <Button variant="outline" disabled={logoutMutation.isPending} onClick={() => setLogoutDialogOpen(true)}>
              <LogOut className="h-4 w-4" />
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
      <ConfirmDialog
        open={logoutDialogOpen}
        onOpenChange={setLogoutDialogOpen}
        title="Dang xuat khoi phien lam viec?"
        description="Frontend se xoa access token trong memory va refresh token trong session storage, sau do dua ban ve trang dang nhap."
        confirmLabel="Dang xuat"
        onConfirm={() => logoutMutation.mutate()}
        pending={logoutMutation.isPending}
        tone="destructive"
      />
    </>
  );
}