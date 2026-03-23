"use client";

import { zodResolver } from "@hookform/resolvers/zod";
import { useQuery } from "@tanstack/react-query";
import { Clock3, MapPinned } from "lucide-react";
import { useEffect, useRef, useState } from "react";
import { useForm } from "react-hook-form";

import { EmptyState } from "@/components/feedback/empty-state";
import { ErrorState } from "@/components/feedback/error-state";
import { PageLoadingState } from "@/components/feedback/page-loading-state";
import { Button } from "@/components/ui/button";
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "@/components/ui/card";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { StatusBadge } from "@/components/ui/status-badge";
import { getTrackingSummary, getTrackingTimeline } from "@/lib/api/client";
import { formatDate } from "@/lib/utils";
import { trackingLookupSchema, type TrackingLookupSchema } from "@/lib/validation/search";

export default function TrackingPage() {
  const [trackingCode, setTrackingCode] = useState("");
  const trackingInputRef = useRef<HTMLInputElement | null>(null);
  const form = useForm<TrackingLookupSchema>({
    resolver: zodResolver(trackingLookupSchema),
    defaultValues: { trackingCode: "" },
  });
  const trackingField = form.register("trackingCode");

  const summaryQuery = useQuery({
    queryKey: ["tracking-summary", trackingCode],
    queryFn: () => getTrackingSummary(trackingCode),
    enabled: Boolean(trackingCode),
  });

  const timelineQuery = useQuery({
    queryKey: ["tracking-timeline", trackingCode],
    queryFn: () => getTrackingTimeline(trackingCode),
    enabled: Boolean(trackingCode),
  });

  useEffect(() => {
    const handleKeyDown = (event: KeyboardEvent) => {
      if (event.key !== "/" || event.metaKey || event.ctrlKey || event.altKey) {
        return;
      }

      const target = event.target;

      if (target instanceof HTMLElement && (target.tagName === "INPUT" || target.tagName === "TEXTAREA" || target.isContentEditable)) {
        return;
      }

      event.preventDefault();
      trackingInputRef.current?.focus();
    };

    window.addEventListener("keydown", handleKeyDown);
    return () => window.removeEventListener("keydown", handleKeyDown);
  }, []);

  return (
    <div className="grid gap-6">
      <div className="page-header">
        <div className="space-y-3">
          <p className="section-kicker">Tra cứu</p>
          <h2 className="text-3xl font-semibold sm:text-4xl">Tra cứu vận đơn</h2>
          <p className="page-copy">Nhập mã vận đơn để xem trạng thái và hành trình giao hàng. Nhấn <kbd className="keyboard-hint">/</kbd> để tập trung vào ô tìm kiếm.</p>
        </div>
        <div className="keyboard-hint">/</div>
      </div>

      <Card>
        <CardHeader>
          <CardTitle>Tra cứu theo mã vận đơn</CardTitle>
          <CardDescription>Nhập mã vận đơn để xem thông tin và hành trình giao hàng.</CardDescription>
        </CardHeader>
        <CardContent>
          <form className="grid gap-4 md:grid-cols-[1fr_auto]" onSubmit={form.handleSubmit((values) => setTrackingCode(values.trackingCode))}>
            <div className="space-y-2">
              <Label htmlFor="tracking-code">Mã vận đơn</Label>
              <Input
                id="tracking-code"
                {...trackingField}
                ref={(node) => {
                  trackingField.ref(node);
                  trackingInputRef.current = node;
                }}
                placeholder="Nhập mã vận đơn (ví dụ: LGA...)"
              />
              {form.formState.errors.trackingCode ? (
                <p className="text-sm text-destructive">{form.formState.errors.trackingCode.message}</p>
              ) : null}
            </div>
            <div className="flex items-end">
              <Button type="submit">Tra cuu</Button>
            </div>
          </form>
        </CardContent>
      </Card>

      {summaryQuery.isError || timelineQuery.isError ? (
        <ErrorState
          description={(summaryQuery.error ?? timelineQuery.error) instanceof Error ? (summaryQuery.error ?? timelineQuery.error)?.message ?? "Không thể tải thông tin vận đơn." : "Không thể tải thông tin vận đơn."}
          onRetry={() => {
            void summaryQuery.refetch();
            void timelineQuery.refetch();
          }}
        />
      ) : null}

      {summaryQuery.isLoading || timelineQuery.isLoading ? <PageLoadingState variant="tracking" /> : null}

      {summaryQuery.data ? (
        <Card>
          <CardHeader>
            <CardTitle className="flex items-center gap-3">
              {summaryQuery.data.trackingCode}
              <StatusBadge status={summaryQuery.data.currentStatus} />
            </CardTitle>
            <CardDescription>{summaryQuery.data.shipmentCode} · Người nhận: {summaryQuery.data.receiverName}</CardDescription>
          </CardHeader>
          <CardContent>
            <p className="text-sm text-muted-foreground">Cập nhật lần cuối: {formatDate(summaryQuery.data.lastUpdatedAt)}</p>
          </CardContent>
        </Card>
      ) : null}

      {timelineQuery.data?.events.length ? (
        <Card>
          <CardHeader>
            <CardTitle>Hành trình vận chuyển</CardTitle>
            <CardDescription>Lịch sử các sự kiện giao hàng theo thời gian.</CardDescription>
          </CardHeader>
          <CardContent className="space-y-4">
            {timelineQuery.data.events.map((event, index) => (
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
            ))}
          </CardContent>
        </Card>
      ) : null}

      {timelineQuery.isSuccess && !timelineQuery.data.events.length ? (
        <EmptyState icon={MapPinned} title="Chưa có hành trình" description="Mã vận đơn hợp lệ nhưng chưa có sự kiện giao hàng nào." variant="tracking" />
      ) : null}

      {!trackingCode && !summaryQuery.isFetching ? (
        <EmptyState icon={MapPinned} title="Nhập mã vận đơn" description="Tra cứu không yêu cầu đăng nhập. Nhập mã vận đơn để xem trạng thái và hành trình giao hàng." variant="tracking" />
      ) : null}
    </div>
  );
}