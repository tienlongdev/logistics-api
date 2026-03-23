import { Skeleton } from "@/components/ui/skeleton";

export default function RootLoading() {
  return (
    <div className="page-shell grid gap-6">
      <Skeleton className="h-12 w-48" />
      <div className="grid gap-4 md:grid-cols-3">
        <Skeleton className="h-36 w-full" />
        <Skeleton className="h-36 w-full" />
        <Skeleton className="h-36 w-full" />
      </div>
      <Skeleton className="h-[420px] w-full" />
    </div>
  );
}