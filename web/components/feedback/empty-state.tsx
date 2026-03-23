import type { LucideIcon } from "lucide-react";

import { Button } from "@/components/ui/button";
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "@/components/ui/card";

type EmptyStateVariant = "generic" | "search" | "tracking" | "shipments";

interface EmptyStateProps {
  icon: LucideIcon;
  title: string;
  description: string;
  actionLabel?: string;
  onAction?: () => void;
  variant?: EmptyStateVariant;
}

function EmptyStateIllustration({ variant }: { variant: EmptyStateVariant }) {
  if (variant === "tracking") {
    return (
      <svg viewBox="0 0 180 120" className="h-28 w-full max-w-[12rem] text-primary" aria-hidden="true">
        <path d="M18 84h144" fill="none" stroke="currentColor" strokeOpacity="0.18" strokeWidth="6" strokeLinecap="round" />
        <path d="M34 70c8-18 25-28 51-28 19 0 34 5 44 15" fill="none" stroke="currentColor" strokeWidth="6" strokeLinecap="round" />
        <circle cx="34" cy="70" r="11" fill="currentColor" fillOpacity="0.14" stroke="currentColor" strokeWidth="4" />
        <circle cx="126" cy="56" r="11" fill="currentColor" fillOpacity="0.14" stroke="currentColor" strokeWidth="4" />
        <circle cx="150" cy="84" r="8" fill="currentColor" />
      </svg>
    );
  }

  if (variant === "shipments") {
    return (
      <svg viewBox="0 0 180 120" className="h-28 w-full max-w-[12rem] text-primary" aria-hidden="true">
        <rect x="26" y="34" width="54" height="40" rx="12" fill="currentColor" fillOpacity="0.12" stroke="currentColor" strokeWidth="4" />
        <rect x="78" y="46" width="76" height="48" rx="14" fill="currentColor" fillOpacity="0.08" stroke="currentColor" strokeWidth="4" />
        <path d="M52 34v-12l18-8 18 8v12" fill="none" stroke="currentColor" strokeWidth="4" strokeLinecap="round" strokeLinejoin="round" />
      </svg>
    );
  }

  if (variant === "search") {
    return (
      <svg viewBox="0 0 180 120" className="h-28 w-full max-w-[12rem] text-primary" aria-hidden="true">
        <circle cx="76" cy="54" r="26" fill="currentColor" fillOpacity="0.1" stroke="currentColor" strokeWidth="4" />
        <path d="M96 74l28 28" fill="none" stroke="currentColor" strokeWidth="6" strokeLinecap="round" />
        <path d="M58 54h36" fill="none" stroke="currentColor" strokeOpacity="0.3" strokeWidth="5" strokeLinecap="round" />
      </svg>
    );
  }

  return (
    <svg viewBox="0 0 180 120" className="h-28 w-full max-w-[12rem] text-primary" aria-hidden="true">
      <rect x="22" y="20" width="136" height="80" rx="18" fill="currentColor" fillOpacity="0.08" stroke="currentColor" strokeWidth="4" />
      <path d="M52 48h76M52 70h48" fill="none" stroke="currentColor" strokeOpacity="0.28" strokeWidth="6" strokeLinecap="round" />
    </svg>
  );
}

export function EmptyState({ icon: Icon, title, description, actionLabel, onAction, variant = "generic" }: EmptyStateProps) {
  return (
    <Card className="relative overflow-hidden">
      <div className="absolute inset-x-0 top-0 h-20 bg-gradient-to-r from-primary/10 via-transparent to-transparent" />
      <CardHeader className="items-start gap-4 sm:flex-row sm:justify-between">
        <div>
          <div className="mb-4 flex h-12 w-12 items-center justify-center rounded-2xl bg-accent text-accent-foreground">
          <Icon className="h-6 w-6" />
          </div>
          <CardTitle>{title}</CardTitle>
          <CardDescription className="mt-2 max-w-xl">{description}</CardDescription>
        </div>
        <div className="flex w-full justify-center sm:w-auto"> 
          <EmptyStateIllustration variant={variant} />
        </div>
      </CardHeader>
      {actionLabel && onAction ? (
        <CardContent>
          <Button type="button" onClick={onAction}>
            {actionLabel}
          </Button>
        </CardContent>
      ) : null}
    </Card>
  );
}