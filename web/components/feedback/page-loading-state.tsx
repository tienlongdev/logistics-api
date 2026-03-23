import { Skeleton } from "@/components/ui/skeleton";

export function PageLoadingState() {
  return (
    <div className="grid gap-4">
      <div className="grid gap-4 md:grid-cols-3">
        <Skeleton className="h-32 w-full" />
        <Skeleton className="h-32 w-full" />
        <Skeleton className="h-32 w-full" />
      </div>
      <Skeleton className="h-[360px] w-full" />
    </div>
  );
}