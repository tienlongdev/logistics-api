"use client";

import { zodResolver } from "@hookform/resolvers/zod";
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { ArrowLeft, Clock3, Package2, Phone, RefreshCcw, Route, UserRound } from "lucide-react";
import Link from "next/link";
import { useParams, useSearchParams } from "next/navigation";
import { useState } from "react";
import { useForm } from "react-hook-form";
import { toast } from "sonner";

import { BackendNotAvailable } from "@/components/feedback/backend-not-available";
import { ConfirmDialog } from "@/components/feedback/confirm-dialog";
import { EmptyState } from "@/components/feedback/empty-state";
import { ErrorState } from "@/components/feedback/error-state";
import { PageLoadingState } from "@/components/feedback/page-loading-state";
import { Button } from "@/components/ui/button";
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "@/components/ui/card";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { StatusBadge } from "@/components/ui/status-badge";
import { Textarea } from "@/components/ui/textarea";
import {
    getTrackingSummary,
    getTrackingTimeline,
    transitionShipmentStatus,
} from "@/lib/api/client";
import { privilegedRoles, shipmentStatusOptions } from "@/lib/constants/status";
import { useAuthStore } from "@/lib/store/auth-store";
import type { ShipmentStatus } from "@/lib/types/api";
import { formatCurrency, formatDate, isUuid } from "@/lib/utils";
import { type ShipmentStatusTransitionSchema, shipmentStatusTransitionSchema } from "@/lib/validation/shipments";

function readNumber(value: string | null) {
  if (!value) {
    return null;
  }

  const parsed = Number(value);
  return Number.isFinite(parsed) ? parsed : null;
}

export default function ShipmentDetailPage() {
  const params = useParams<{ id: string }>();
  const searchParams = useSearchParams();
  const queryClient = useQueryClient();
  const accessToken = useAuthStore((state) => state.accessToken ?? undefined);
  const roles = useAuthStore((state) => state.roles);

  const routeId = params.id;
  const shipmentId = isUuid(routeId) ? routeId : null;
  const trackingCode = isUuid(routeId) ? searchParams.get("trackingCode") : routeId;
  const shipmentCode = searchParams.get("shipmentCode");
  const receiverName = searchParams.get("receiverName");
  const receiverPhone = searchParams.get("receiverPhone");
  const senderName = searchParams.get("senderName");
  const createdAt = searchParams.get("createdAt");
  const codAmount = readNumber(searchParams.get("codAmount"));
  const shippingFee = readNumber(searchParams.get("shippingFee"));
  const totalFee = readNumber(searchParams.get("totalFee"));
  const canTransitionStatus = Boolean(shipmentId && roles.some((role) => privilegedRoles.has(role)));
  const [confirmOpen, setConfirmOpen] = useState(false);

  const transitionForm = useForm<ShipmentStatusTransitionSchema>({
    resolver: zodResolver(shipmentStatusTransitionSchema),
    defaultValues: {
      hubCode: "",
      location: "",
      note: "",
      occurredAt: "",
      toStatus: "PickedUp",
    },
  });

  const summaryQuery = useQuery({
    queryKey: ["shipment-detail", "tracking-summary", trackingCode],
    queryFn: () => getTrackingSummary(trackingCode ?? ""),
    enabled: Boolean(trackingCode),
  });

  const timelineQuery = useQuery({
    queryKey: ["shipment-detail", "tracking-timeline", trackingCode],
    queryFn: () => getTrackingTimeline(trackingCode ?? ""),
    enabled: Boolean(trackingCode),
  });

  const transitionMutation = useMutation({
    mutationFn: (values: ShipmentStatusTransitionSchema) =>
      transitionShipmentStatus(
        shipmentId ?? "",
        {
          hubCode: values.hubCode || null,
          location: values.location || null,
          note: values.note || null,
          occurredAt: values.occurredAt ? new Date(values.occurredAt).toISOString() : null,
          toStatus: values.toStatus,
        },
        accessToken,
      ),
    onSuccess: async () => {
      await Promise.all([
        queryClient.invalidateQueries({ queryKey: ["shipment-detail", "tracking-summary", trackingCode] }),
        queryClient.invalidateQueries({ queryKey: ["shipment-detail", "tracking-timeline", trackingCode] }),
      ]);
      setConfirmOpen(false);
      toast.success("Da cap nhat trang thai shipment");
    },
  });

  const previewStatus = searchParams.get("status") as ShipmentStatus | null;
  const currentStatus = summaryQuery.data?.currentStatus ?? previewStatus;

  return (
    <div className="grid gap-6">
      <div className="page-header">
        <div className="space-y-3">
          <Button asChild variant="ghost" className="w-fit px-0 text-muted-foreground">
            <Link href="/shipments">
              <ArrowLeft className="h-4 w-4" />
              Quay lai shipments
            </Link>
          </Button>
          <div>
            <p className="text-sm font-medium uppercase tracking-[0.2em] text-primary">Shipment detail</p>
            <div className="mt-2 flex flex-wrap items-center gap-3">
              <h2 className="text-3xl font-semibold">{trackingCode ?? routeId}</h2>
              {currentStatus ? <StatusBadge status={currentStatus} /> : null}
            </div>
            <p className="mt-2 text-sm text-muted-foreground">
              Shipment detail API chua duoc implement. Trang nay ket hop real tracking endpoints voi preview data co san tu search read model.
            </p>
          </div>
        </div>
      </div>

      <BackendNotAvailable
        description="Missing `GET /api/v1/shipments/{id}` and `GET /api/v1/shipments/by-tracking/{trackingCode}` in backend."
        todo="TODO: follow docs/tasks.md item E5 to implement shipment query APIs, then replace search-parameter preview blocks with real shipment detail DTOs."
      />

      {summaryQuery.isError || timelineQuery.isError ? (
        <ErrorState
          description={(summaryQuery.error ?? timelineQuery.error) instanceof Error ? (summaryQuery.error ?? timelineQuery.error)?.message ?? "Khong the tai shipment tracking." : "Khong the tai shipment tracking."}
          onRetry={() => {
            void summaryQuery.refetch();
            void timelineQuery.refetch();
          }}
        />
      ) : null}

      {summaryQuery.isLoading || timelineQuery.isLoading ? <PageLoadingState variant="detail" /> : null}

      <section className="grid gap-4 xl:grid-cols-[1.35fr_0.65fr]">
        <div className="grid gap-4">
          <Card>
            <CardHeader>
              <CardTitle className="flex items-center gap-2">
                <Route className="h-5 w-5 text-primary" />
                Tracking timeline
              </CardTitle>
              <CardDescription>Public tracking endpoints duoc dung lam timeline chinh cho shipment detail.</CardDescription>
            </CardHeader>
            <CardContent className="space-y-4">
              {timelineQuery.data?.events.length ? (
                timelineQuery.data.events.map((event, index) => (
                  <div key={event.eventId} className="relative flex gap-4 pl-8">
                    <div className="absolute left-0 top-1 flex h-5 w-5 items-center justify-center rounded-full bg-primary/10 text-primary">
                      <Clock3 className="h-3 w-3" />
                    </div>
                    {index < timelineQuery.data.events.length - 1 ? <div className="absolute left-2.5 top-6 h-[calc(100%+0.5rem)] w-px bg-border" /> : null}
                    <div className="w-full rounded-2xl border border-border/70 bg-background/70 p-4">
                      <div className="flex flex-wrap items-center gap-2">
                        <StatusBadge status={event.toStatus} />
                        <span className="text-sm text-muted-foreground">{formatDate(event.occurredAt)}</span>
                      </div>
                      <p className="mt-2 text-sm">{event.note ?? "Khong co ghi chu"}</p>
                      <p className="mt-2 text-xs text-muted-foreground">{event.location ?? event.hubCode ?? "No hub data"}</p>
                    </div>
                  </div>
                ))
              ) : timelineQuery.isSuccess ? (
                <EmptyState icon={Route} title="Chua co timeline" description="Backend tracking co summary nhung chua tra event nao." variant="tracking" />
              ) : null}
            </CardContent>
          </Card>

          <div className="grid gap-4 md:grid-cols-2">
            <Card>
              <CardHeader>
                <CardTitle className="flex items-center gap-2 text-base">
                  <UserRound className="h-4 w-4 text-primary" />
                  Sender / Receiver
                </CardTitle>
              </CardHeader>
              <CardContent className="space-y-4 text-sm">
                <div>
                  <p className="text-xs uppercase tracking-[0.18em] text-muted-foreground">Sender</p>
                  <p className="mt-1 font-medium">{senderName ?? "Backend not available"}</p>
                </div>
                <div>
                  <p className="text-xs uppercase tracking-[0.18em] text-muted-foreground">Receiver</p>
                  <p className="mt-1 font-medium">{summaryQuery.data?.receiverName ?? receiverName ?? "Backend not available"}</p>
                  <p className="mt-1 flex items-center gap-2 text-muted-foreground">
                    <Phone className="h-3.5 w-3.5" />
                    {receiverPhone ?? "Backend not available"}
                  </p>
                </div>
              </CardContent>
            </Card>

            <Card>
              <CardHeader>
                <CardTitle className="flex items-center gap-2 text-base">
                  <Package2 className="h-4 w-4 text-primary" />
                  Package / Fees
                </CardTitle>
              </CardHeader>
              <CardContent className="space-y-4 text-sm">
                <div>
                  <p className="text-xs uppercase tracking-[0.18em] text-muted-foreground">Package</p>
                  <p className="mt-1 font-medium text-muted-foreground">Backend not available</p>
                  <p className="mt-1 text-muted-foreground">TODO: docs/tasks.md E5 can mo rong DTO de co weight, dimensions, description.</p>
                </div>
                <div className="grid gap-2 sm:grid-cols-3">
                  <div className="rounded-2xl bg-accent/60 p-3">
                    <p className="text-xs uppercase tracking-[0.14em] text-muted-foreground">COD</p>
                    <p className="mt-1 font-medium">{codAmount !== null ? formatCurrency(codAmount) : "Backend not available"}</p>
                  </div>
                  <div className="rounded-2xl bg-accent/60 p-3">
                    <p className="text-xs uppercase tracking-[0.14em] text-muted-foreground">Shipping fee</p>
                    <p className="mt-1 font-medium">{shippingFee !== null ? formatCurrency(shippingFee) : "Backend not available"}</p>
                  </div>
                  <div className="rounded-2xl bg-accent/60 p-3">
                    <p className="text-xs uppercase tracking-[0.14em] text-muted-foreground">Total fee</p>
                    <p className="mt-1 font-medium">{totalFee !== null ? formatCurrency(totalFee) : "Backend not available"}</p>
                  </div>
                </div>
              </CardContent>
            </Card>
          </div>
        </div>

        <div className="grid gap-4">
          <Card>
            <CardHeader>
              <CardTitle>Shipment snapshot</CardTitle>
              <CardDescription>Du lieu co the doc duoc ngay tu tracking va search response.</CardDescription>
            </CardHeader>
            <CardContent className="space-y-3 text-sm">
              <div className="flex items-center justify-between gap-4">
                <span className="text-muted-foreground">Shipment code</span>
                <span className="text-right font-medium">{summaryQuery.data?.shipmentCode ?? shipmentCode ?? "Backend not available"}</span>
              </div>
              <div className="flex items-center justify-between gap-4">
                <span className="text-muted-foreground">Created at</span>
                <span className="text-right font-medium">{createdAt ? formatDate(createdAt) : "Backend not available"}</span>
              </div>
              <div className="flex items-center justify-between gap-4">
                <span className="text-muted-foreground">Status source</span>
                <span className="text-right font-medium">{summaryQuery.data ? "Tracking API" : "Search preview"}</span>
              </div>
            </CardContent>
          </Card>

          <BackendNotAvailable
            description="`POST /api/v1/shipments/{id}/cancel` chua ton tai trong backend hien tai."
            todo="TODO: docs/tasks.md item E3 can them dedicated cancel endpoint; tam thoi chi co status transition endpoint tong quat o backend."
          />

          {canTransitionStatus ? (
            <Card>
              <CardHeader>
                <CardTitle>Status update</CardTitle>
                <CardDescription>Form nay goi POST /api/v1/shipments/:id/status-transitions va chi hien khi token co role HubStaff, Operator hoac Admin.</CardDescription>
              </CardHeader>
              <CardContent>
                <form className="grid gap-4" onSubmit={transitionForm.handleSubmit(() => setConfirmOpen(true))}>
                  <div className="space-y-2">
                    <Label htmlFor="toStatus">To status</Label>
                    <select
                      id="toStatus"
                      className="flex h-10 w-full rounded-md border border-input bg-background px-3 py-2 text-sm"
                      {...transitionForm.register("toStatus")}
                    >
                      {shipmentStatusOptions.map((status) => (
                        <option key={status} value={status}>{status}</option>
                      ))}
                    </select>
                  </div>
                  <div className="space-y-2">
                    <Label htmlFor="hubCode">Hub code</Label>
                    <Input id="hubCode" placeholder="HUB-HCM-001" {...transitionForm.register("hubCode")} />
                  </div>
                  <div className="space-y-2">
                    <Label htmlFor="location">Location</Label>
                    <Input id="location" placeholder="Thu Duc, HCM" {...transitionForm.register("location")} />
                  </div>
                  <div className="space-y-2">
                    <Label htmlFor="occurredAt">Occurred at</Label>
                    <Input id="occurredAt" type="datetime-local" {...transitionForm.register("occurredAt")} />
                  </div>
                  <div className="space-y-2">
                    <Label htmlFor="note">Note</Label>
                    <Textarea id="note" placeholder="Da lay hang" {...transitionForm.register("note")} />
                  </div>
                  <Button type="submit" disabled={transitionMutation.isPending}>
                    {transitionMutation.isPending ? "Dang cap nhat..." : "Cap nhat trang thai"}
                  </Button>
                </form>
              </CardContent>
            </Card>
          ) : (
            <Card>
              <CardHeader>
                <CardTitle>Status update</CardTitle>
                <CardDescription>
                  Endpoint status transition co san, nhung UI chi cho phep goi khi co shipment id UUID va token mang role `HubStaff`, `Operator` hoac `Admin`.
                </CardDescription>
              </CardHeader>
              <CardContent>
                <p className="text-sm text-muted-foreground">
                  Mo shipment detail tu bang shipments/search de co `shipmentId`, sau do dang nhap bang tai khoan van hanh co quyen phu hop.
                </p>
              </CardContent>
            </Card>
          )}

          {transitionMutation.isError ? (
            <ErrorState description={transitionMutation.error instanceof Error ? transitionMutation.error.message : "Khong the cap nhat trang thai."} onRetry={() => transitionForm.handleSubmit((values) => transitionMutation.mutate(values))()} />
          ) : null}
          {transitionMutation.isSuccess ? (
            <Card className="border-emerald-300/70 bg-emerald-50/70 shadow-none dark:border-emerald-700/60 dark:bg-emerald-950/20">
              <CardContent className="flex items-center gap-3 p-4 text-sm text-emerald-950 dark:text-emerald-100">
                <RefreshCcw className="h-4 w-4" />
                Trang thai da duoc cap nhat. Timeline va summary da refetch.
              </CardContent>
            </Card>
          ) : null}
        </div>
      </section>

      <ConfirmDialog
        open={confirmOpen}
        onOpenChange={setConfirmOpen}
        title="Xac nhan cap nhat trang thai?"
        description="Thao tac nay se goi shipment status transition endpoint hien co. Chi tiep tuc neu hub code, location va thoi diem da dung."
        confirmLabel="Xac nhan cap nhat"
        onConfirm={() => transitionForm.handleSubmit((values) => transitionMutation.mutate(values))()}
        pending={transitionMutation.isPending}
      />
    </div>
  );
}