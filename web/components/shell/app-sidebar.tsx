"use client";

import { Boxes, Sparkles } from "lucide-react";
import Link from "next/link";
import { usePathname } from "next/navigation";

import { Badge } from "@/components/ui/badge";
import { appNavigation } from "@/lib/constants/navigation";
import { cn } from "@/lib/utils";

interface AppSidebarProps {
  onNavigate?: () => void;
}

export function AppSidebar({ onNavigate }: AppSidebarProps) {
  const pathname = usePathname();

  return (
    <div className="flex h-full flex-col">
      <div className="mb-8 flex items-center gap-3">
        <div className="flex h-11 w-11 items-center justify-center rounded-2xl bg-primary text-primary-foreground shadow-lg shadow-primary/25">
          <Boxes className="h-5 w-5" />
        </div>
        <div>
          <p className="text-sm font-semibold">Logistics Web</p>
          <p className="text-xs text-muted-foreground">Merchant operations console</p>
        </div>
      </div>

      <div className="mb-6 rounded-2xl border border-border/70 bg-accent/60 p-4">
        <div className="mb-2 flex items-center gap-2 text-sm font-medium text-accent-foreground">
          <Sparkles className="h-4 w-4" />
          Modern SaaS shell
        </div>
        <p className="text-sm text-muted-foreground">Sidebar desktop, drawer mobile, tokenized theme va query cache san sang cho backend.</p>
      </div>

      <nav className="space-y-1" aria-label="Primary navigation">
        {appNavigation.map((item) => {
          const isActive = pathname === item.href;
          const Icon = item.icon;

          return (
            <Link
              key={item.href}
              href={item.href}
              onClick={onNavigate}
              className={cn(
                "group flex items-start gap-3 rounded-2xl px-3 py-3 transition-colors",
                isActive ? "bg-primary text-primary-foreground" : "hover:bg-accent",
              )}
            >
              <Icon className="mt-0.5 h-4 w-4 shrink-0" />
              <div className="min-w-0">
                <div className="flex items-center gap-2">
                  <span className="text-sm font-medium">{item.label}</span>
                  {isActive ? <Badge variant="outline" className="border-white/30 text-current">Live</Badge> : null}
                </div>
                <p className={cn("text-xs", isActive ? "text-primary-foreground/80" : "text-muted-foreground")}>{item.description}</p>
              </div>
            </Link>
          );
        })}
      </nav>

      <div className="mt-auto rounded-2xl border border-dashed border-border p-4">
        <p className="text-sm font-medium">API-aware UI</p>
        <p className="mt-1 text-sm text-muted-foreground">Search va tracking da map theo contract trong `/docs/api-contract.md`.</p>
      </div>
    </div>
  );
}