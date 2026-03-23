"use client";

import { zodResolver } from "@hookform/resolvers/zod";
import { useQuery } from "@tanstack/react-query";
import { Clock3, MapPinned } from "lucide-react";
import { useState } from "react";
import { useForm } from "react-hook-form";

import { EmptyState } from "@/components/feedback/empty-state";
import { ErrorState } from "@/components/feedback/error-state";
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
  const form = useForm<TrackingLookupSchema>({
    resolver: zodResolver(trackingLookupSchema),
    defaultValues: { trackingCode: "" },
  });

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

  return (
    <div className="grid gap-6">
      <div className="page-header">
        <div>
          <p className="text-sm font-medium uppercase tracking-[0.2em] text-primary">Tracking</p>
          <h2 className="text-3xl font-semibold">Public timeline lookup</h2>
        </div>
        <p className="max-w-xl text-sm text-muted-foreground">Route nay public, mobile-first, va chi dung `GET /api/v1/tracking/{trackingCode}` cung timeline endpoint tu backend.</p>
      </div>

      <Card>
        <CardHeader>
          <CardTitle>Tracking lookup</CardTitle>
          <CardDescription>Nhap tracking code de lay summary va timeline immutable.</CardDescription>
        </CardHeader>
        <CardContent>
          <form className="grid gap-4 md:grid-cols-[1fr_auto]" onSubmit={form.handleSubmit((values) => setTrackingCode(values.trackingCode))}>
            <div className="space-y-2">
              <Label htmlFor="tracking-code">Tracking code</Label>
              <Input id="tracking-code" {...form.register("trackingCode")} />
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
          description={(summaryQuery.error ?? timelineQuery.error) instanceof Error ? (summaryQuery.error ?? timelineQuery.error)?.message ?? "Khong the tai tracking data." : "Khong the tai tracking data."}
          onRetry={() => {
            void summaryQuery.refetch();
            void timelineQuery.refetch();
          }}
        />
      ) : null}

      {summaryQuery.data ? (
        <Card>
          <CardHeader>
            <CardTitle className="flex items-center gap-3">
              {summaryQuery.data.trackingCode}
              <StatusBadge status={summaryQuery.data.currentStatus} />
            </CardTitle>
            <CardDescription>{summaryQuery.data.shipmentCode} · Receiver {summaryQuery.data.receiverName}</CardDescription>
          </CardHeader>
          <CardContent>
            <p className="text-sm text-muted-foreground">Cap nhat lan cuoi {formatDate(summaryQuery.data.lastUpdatedAt)}</p>
          </CardContent>
        </Card>
      ) : null}

      {timelineQuery.data?.events.length ? (
        <Card>
          <CardHeader>
            <CardTitle>Tracking timeline</CardTitle>
            <CardDescription>Events duoc sap xep tu cu nhat den moi nhat theo contract.</CardDescription>
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
                  <p className="mt-2 text-sm">{event.note ?? "Khong co ghi chu"}</p>
                  <p className="mt-2 text-xs text-muted-foreground">{event.location ?? event.hubCode ?? "No hub data"}</p>
                </div>
              </div>
            ))}
          </CardContent>
        </Card>
      ) : null}

      {timelineQuery.isSuccess && !timelineQuery.data.events.length ? (
        <EmptyState icon={MapPinned} title="Chua co timeline" description="Tracking code hop le nhung backend chua tra ve event nao." />
      ) : null}

      {!trackingCode && !summaryQuery.isFetching ? (
        <EmptyState icon={MapPinned} title="Nhap tracking code" description="Public tracking route khong can dang nhap. Nhap ma van don de xem status va timeline." />
      ) : null}
    </div>
  );
}