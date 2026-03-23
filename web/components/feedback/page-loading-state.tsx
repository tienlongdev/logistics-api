import { Skeleton } from "@/components/ui/skeleton";

interface PageLoadingStateProps {
  variant?: "dashboard" | "list" | "detail" | "tracking";
}

export function PageLoadingState({ variant = "dashboard" }: PageLoadingStateProps) {
  if (variant === "tracking") {
    return (
      <div className="grid gap-4">
        <Skeleton className="h-36 w-full" />
        <Skeleton className="h-24 w-full" />
        <div className="grid gap-3">
          <Skeleton className="h-24 w-full" />
          <Skeleton className="h-24 w-full" />
          <Skeleton className="h-24 w-full" />
        </div>
      </div>
    );
  }

  if (variant === "detail") {
    return (
      <div className="grid gap-4 xl:grid-cols-[1.35fr_0.65fr]">
        <div className="grid gap-4">
          <Skeleton className="h-[360px] w-full" />
          <div className="grid gap-4 md:grid-cols-2">
            <Skeleton className="h-56 w-full" />
            <Skeleton className="h-56 w-full" />
          </div>
        </div>
        <div className="grid gap-4">
          <Skeleton className="h-48 w-full" />
          <Skeleton className="h-72 w-full" />
        </div>
      </div>
    );
  }

  if (variant === "list") {
    return (
      <div className="grid gap-4">
        <Skeleton className="h-44 w-full" />
        <Skeleton className="h-[420px] w-full" />
      </div>
    );
  }

  return (
    <div className="grid gap-4">
      <div className="grid gap-4 md:grid-cols-2 xl:grid-cols-4">
        <Skeleton className="h-32 w-full" />
        <Skeleton className="h-32 w-full" />
        <Skeleton className="h-32 w-full" />
        <Skeleton className="h-32 w-full" />
      </div>
      <Skeleton className="h-[360px] w-full" />
    </div>
  );
}