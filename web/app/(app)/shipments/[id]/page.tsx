"use client";

import { zodResolver } from "@hookform/resolvers/zod";
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { ArrowLeft, Clock3, Package2, Phone, RefreshCcw, Route, UserRound } from "lucide-react";
import Link from "next/link";
import { useParams, useSearchParams } from "next/navigation";
import { useState } from "react";
import { useForm } from "react-hook-form";
import { toast } from "sonner";

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
import { privilegedRoles, shipmentStatusLabel, shipmentStatusOptions } from "@/lib/constants/status";
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
      toast.success("Đã cập nhật trạng thái đơn hàng");
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
              Quay lại danh sách đơn hàng
            </Link>
          </Button>
          <div>
            <p className="text-sm font-medium uppercase tracking-[0.2em] text-primary">Chi tiết đơn hàng</p>
            <div className="mt-2 flex flex-wrap items-center gap-3">
              <h2 className="text-3xl font-semibold">{trackingCode ?? routeId}</h2>
              {currentStatus ? <StatusBadge status={currentStatus} /> : null}
            </div>
          </div>
        </div>
      </div>

      {summaryQuery.isError || timelineQuery.isError ? (
        <ErrorState
          description={(summaryQuery.error ?? timelineQuery.error) instanceof Error ? (summaryQuery.error ?? timelineQuery.error)?.message ?? "Không thể tải thông tin vận đơn." : "Không thể tải thông tin vận đơn."}
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
                Hành trình vận chuyển
              </CardTitle>
              <CardDescription>Lịch sử hành trình giao hàng theo thời gian.</CardDescription>
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
                      <p className="mt-2 text-sm">{event.note ?? "Không có ghi chú"}</p>
                      <p className="mt-2 text-xs text-muted-foreground">{event.location ?? event.hubCode ?? "Không có thông tin vị trí"}</p>
                    </div>
                  </div>
                ))
              ) : timelineQuery.isSuccess ? (
                <EmptyState icon={Route} title="Chưa có hành trình" description="Đơn hàng chưa có sự kiện vận chuyển nào." variant="tracking" />
              ) : null}
            </CardContent>
          </Card>

          <div className="grid gap-4 md:grid-cols-2">
            <Card>
              <CardHeader>
                <CardTitle className="flex items-center gap-2 text-base">
                  <UserRound className="h-4 w-4 text-primary" />
                  Người gửi / Người nhận
                </CardTitle>
              </CardHeader>
              <CardContent className="space-y-4 text-sm">
                <div>
                  <p className="text-xs uppercase tracking-[0.18em] text-muted-foreground">Người gửi</p>
                  <p className="mt-1 font-medium">{senderName ?? "Đang tải..."}</p>
                </div>
                <div>
                  <p className="text-xs uppercase tracking-[0.18em] text-muted-foreground">Người nhận</p>
                  <p className="mt-1 font-medium">{summaryQuery.data?.receiverName ?? receiverName ?? "Đang tải..."}</p>
                  <p className="mt-1 flex items-center gap-2 text-muted-foreground">
                    <Phone className="h-3.5 w-3.5" />
                    {receiverPhone ?? "Đang tải..."}
                  </p>
                </div>
              </CardContent>
            </Card>

            <Card>
              <CardHeader>
                <CardTitle className="flex items-center gap-2 text-base">
                  <Package2 className="h-4 w-4 text-primary" />
                  Thông tin đơn hàng
                </CardTitle>
              </CardHeader>
              <CardContent className="space-y-4 text-sm">
                <div className="grid gap-2 sm:grid-cols-3">
                  <div className="rounded-2xl bg-accent/60 p-3">
                    <p className="text-xs uppercase tracking-[0.14em] text-muted-foreground">COD</p>
                    <p className="mt-1 font-medium">{codAmount !== null ? formatCurrency(codAmount) : "—"}</p>
                  </div>
                  <div className="rounded-2xl bg-accent/60 p-3">
                    <p className="text-xs uppercase tracking-[0.14em] text-muted-foreground">Phí giao</p>
                    <p className="mt-1 font-medium">{shippingFee !== null ? formatCurrency(shippingFee) : "—"}</p>
                  </div>
                  <div className="rounded-2xl bg-accent/60 p-3">
                    <p className="text-xs uppercase tracking-[0.14em] text-muted-foreground">Tổng phí</p>
                    <p className="mt-1 font-medium">{totalFee !== null ? formatCurrency(totalFee) : "—"}</p>
                  </div>
                </div>
              </CardContent>
            </Card>
          </div>
        </div>

        <div className="grid gap-4">
          <Card>
            <CardHeader>
              <CardTitle>Thông tin vận đơn</CardTitle>
            </CardHeader>
            <CardContent className="space-y-3 text-sm">
              <div className="flex items-center justify-between gap-4">
                <span className="text-muted-foreground">Mã đơn hàng</span>
                <span className="text-right font-medium">{summaryQuery.data?.shipmentCode ?? shipmentCode ?? "—"}</span>
              </div>
              <div className="flex items-center justify-between gap-4">
                <span className="text-muted-foreground">Ngày tạo</span>
                <span className="text-right font-medium">{createdAt ? formatDate(createdAt) : "—"}</span>
              </div>
            </CardContent>
          </Card>

          {canTransitionStatus ? (
            <Card>
              <CardHeader>
                <CardTitle>Cập nhật trạng thái</CardTitle>
                <CardDescription>Chỉ dành cho nhân viên vận hành có quyền Hủ đơn / Tiếp nhận / Quản trị.</CardDescription>
              </CardHeader>
              <CardContent>
                <form className="grid gap-4" onSubmit={transitionForm.handleSubmit(() => setConfirmOpen(true))}>
                  <div className="space-y-2">
                    <Label htmlFor="toStatus">Trạng thái mới</Label>
                    <select
                      id="toStatus"
                      className="flex h-10 w-full rounded-md border border-input bg-background px-3 py-2 text-sm"
                      {...transitionForm.register("toStatus")}
                    >
                      {shipmentStatusOptions.map((status) => (
                        <option key={status} value={status}>{shipmentStatusLabel[status]}</option>
                      ))}
                    </select>
                  </div>
                  <div className="space-y-2">
                    <Label htmlFor="hubCode">Mã hub</Label>
                    <Input id="hubCode" placeholder="HUB-HCM-001" {...transitionForm.register("hubCode")} />
                  </div>
                  <div className="space-y-2">
                    <Label htmlFor="location">Vị trí</Label>
                    <Input id="location" placeholder="Thủ Đức, TP.HCM" {...transitionForm.register("location")} />
                  </div>
                  <div className="space-y-2">
                    <Label htmlFor="occurredAt">Thời điểm xảy ra</Label>
                    <Input id="occurredAt" type="datetime-local" {...transitionForm.register("occurredAt")} />
                  </div>
                  <div className="space-y-2">
                    <Label htmlFor="note">Ghi chú</Label>
                    <Textarea id="note" placeholder="Nhập ghi chú..." {...transitionForm.register("note")} />
                  </div>
                  <Button type="submit" disabled={transitionMutation.isPending}>
                    {transitionMutation.isPending ? "Đang cập nhật..." : "Cập nhật trạng thái"}
                  </Button>
                </form>
              </CardContent>
            </Card>
          ) : (
            <Card>
              <CardHeader>
                <CardTitle>Cập nhật trạng thái</CardTitle>
                <CardDescription>
                  Chức năng này chỉ dành cho nhân viên vận hành hợp lệ (HubStaff, Operator, Admin).
                </CardDescription>
              </CardHeader>
              <CardContent>
                <p className="text-sm text-muted-foreground">
                  Liên hệ quản trị để được cấp quyền cập nhật trạng thái đơn hàng.
                </p>
              </CardContent>
            </Card>
          )}

          {transitionMutation.isError ? (
            <ErrorState description={transitionMutation.error instanceof Error ? transitionMutation.error.message : "Không thể cập nhật trạng thái."} onRetry={() => transitionForm.handleSubmit((values) => transitionMutation.mutate(values))()} />
          ) : null}
          {transitionMutation.isSuccess ? (
            <Card className="border-emerald-300/70 bg-emerald-50/70 shadow-none dark:border-emerald-700/60 dark:bg-emerald-950/20">
              <CardContent className="flex items-center gap-3 p-4 text-sm text-emerald-950 dark:text-emerald-100">
                <RefreshCcw className="h-4 w-4" />
                Trạng thái đơn hàng đã được cập nhật.
              </CardContent>
            </Card>
          ) : null}
        </div>
      </section>

      <ConfirmDialog
        open={confirmOpen}
        onOpenChange={setConfirmOpen}
        title="Xác nhận cập nhật trạng thái?"
        description="Bạn có chắc chắn muốn cập nhật trạng thái đơn hàng này không?"
        confirmLabel="Xác nhận"
        onConfirm={() => transitionForm.handleSubmit((values) => transitionMutation.mutate(values))()}
        pending={transitionMutation.isPending}
      />
    </div>
  );
}