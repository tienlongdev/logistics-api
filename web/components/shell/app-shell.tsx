"use client";

import { type ReactNode, useState } from "react";

import { AppSidebar } from "@/components/shell/app-sidebar";
import { AppTopbar } from "@/components/shell/app-topbar";
import { Sheet, SheetContent } from "@/components/ui/sheet";

interface AppShellProps {
  children: ReactNode;
}

export function AppShell({ children }: AppShellProps) {
  const [mobileOpen, setMobileOpen] = useState(false);

  return (
    <div className="min-h-screen lg:grid lg:grid-cols-[var(--sidebar-width)_1fr]">
      <aside className="hidden border-r border-border/70 bg-card/70 px-5 py-6 lg:block">
        <AppSidebar />
      </aside>
      <Sheet open={mobileOpen} onOpenChange={setMobileOpen}>
        <SheetContent className="lg:hidden">
          <AppSidebar onNavigate={() => setMobileOpen(false)} />
        </SheetContent>
      </Sheet>
      <div className="min-w-0">
        <AppTopbar onOpenSidebar={() => setMobileOpen(true)} />
        <main className="page-shell animate-fade-up">{children}</main>
      </div>
    </div>
  );
}