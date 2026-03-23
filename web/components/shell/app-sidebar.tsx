"use client";

import { Boxes } from "lucide-react";
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
          <p className="text-sm font-semibold">Vận chuyển</p>
          <p className="text-xs text-muted-foreground">Hệ thống quản lý đơn hàng</p>
        </div>
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
                "group flex items-start gap-3 rounded-[1.2rem] px-3 py-3 transition-all duration-200",
                isActive ? "bg-primary text-primary-foreground shadow-lg shadow-primary/20" : "hover:bg-accent/80",
              )}
            >
              <Icon className="mt-0.5 h-4 w-4 shrink-0" />
              <div className="min-w-0">
                <div className="flex items-center gap-2">
                  <span className="text-sm font-medium">{item.label}</span>
                  {isActive ? <Badge variant="outline" className="border-white/30 text-current">Đang xem</Badge> : null}
                </div>
                <p className={cn("text-xs", isActive ? "text-primary-foreground/80" : "text-muted-foreground")}>{item.description}</p>
              </div>
            </Link>
          );
        })}
      </nav>

    </div>
  );
}