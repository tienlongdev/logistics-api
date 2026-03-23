"use client";

import { Menu, SearchSlash } from "lucide-react";
import { usePathname, useRouter } from "next/navigation";

import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
import { ThemeToggle } from "@/components/ui/theme-toggle";
import { routeTitles } from "@/lib/constants/navigation";
import { useAuthStore } from "@/lib/store/auth-store";

interface AppTopbarProps {
  onOpenSidebar: () => void;
}

export function AppTopbar({ onOpenSidebar }: AppTopbarProps) {
  const pathname = usePathname();
  const router = useRouter();
  const { clearSession, email, mode } = useAuthStore((state) => ({
    clearSession: state.clearSession,
    email: state.email,
    mode: state.mode,
  }));

  return (
    <header className="sticky top-0 z-30 border-b border-border/70 bg-background/80 backdrop-blur">
      <div className="page-shell flex items-center gap-3 py-4">
        <Button variant="outline" size="icon" className="lg:hidden" onClick={onOpenSidebar} aria-label="Open navigation menu">
          <Menu className="h-4 w-4" />
        </Button>
        <div className="min-w-0 flex-1">
          <div className="flex flex-wrap items-center gap-2">
            <h1 className="truncate text-xl font-semibold">{routeTitles[pathname] ?? "Operations"}</h1>
            {mode ? <Badge variant={mode === "demo" ? "secondary" : "success"}>{mode === "demo" ? "Demo auth" : "API auth"}</Badge> : null}
          </div>
          <p className="text-sm text-muted-foreground">Neutral interface, optimistic shell, and graceful fallback for incomplete backend routes.</p>
        </div>
        <div className="hidden items-center gap-3 md:flex">
          <div className="flex items-center gap-2 rounded-full border border-border bg-card px-3 py-2 text-sm text-muted-foreground">
            <SearchSlash className="h-4 w-4" />
            <span className="max-w-40 truncate">{email ?? "No session"}</span>
          </div>
          <ThemeToggle />
          <Button
            variant="outline"
            onClick={() => {
              clearSession();
              router.push("/login");
            }}
          >
            Dang xuat
          </Button>
        </div>
      </div>
    </header>
  );
}