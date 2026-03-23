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
      <aside className="hidden border-r border-border/70 bg-card/65 px-5 py-6 backdrop-blur lg:block">
        <AppSidebar />
      </aside>
      <Sheet open={mobileOpen} onOpenChange={setMobileOpen}>
        <SheetContent className="lg:hidden">
          <AppSidebar onNavigate={() => setMobileOpen(false)} />
        </SheetContent>
      </Sheet>
      <div className="min-w-0 bg-[linear-gradient(180deg,rgba(255,255,255,0.14),transparent)] dark:bg-[linear-gradient(180deg,rgba(15,23,42,0.18),transparent)]">
        <AppTopbar onOpenSidebar={() => setMobileOpen(true)} />
        <main className="page-shell animate-fade-up">{children}</main>
      </div>
    </div>
  );
}