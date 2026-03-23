import { PageLoadingState } from "@/components/feedback/page-loading-state";

export default function AppLoading() {
  return (
    <div className="grid gap-6">
      <div className="grid gap-2">
        <div className="section-kicker">Loading</div>
        <h2 className="text-3xl font-semibold">Dang chuan bi workspace</h2>
      </div>
      <PageLoadingState variant="dashboard" />
    </div>
  );
}