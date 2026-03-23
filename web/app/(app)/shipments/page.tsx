"use client";

import { keepPreviousData, useQuery } from "@tanstack/react-query";
import { PackageSearch } from "lucide-react";
import { useState } from "react";

import { EmptyState } from "@/components/feedback/empty-state";
import { ErrorState } from "@/components/feedback/error-state";
import { PageLoadingState } from "@/components/feedback/page-loading-state";
import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "@/components/ui/card";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { searchShipments } from "@/lib/api/client";
import { shipmentStatusOptions, shipmentStatusTone } from "@/lib/constants/status";
import { useAuthStore } from "@/lib/store/auth-store";
import type { ShipmentStatus } from "@/lib/types/api";
import { formatCurrency, formatDate } from "@/lib/utils";

export default function ShipmentsPage() {
  const accessToken = useAuthStore((state) => state.accessToken ?? undefined);
  const [trackingCode, setTrackingCode] = useState("");
  const [receiverPhone, setReceiverPhone] = useState("");
  const [status, setStatus] = useState<ShipmentStatus | "">("");

  const query = useQuery({
    queryKey: ["shipments", trackingCode, receiverPhone, status, accessToken],
    queryFn: () =>
      searchShipments(
        {
          trackingCode,
          receiverPhone,
          status,
          page: 1,
          pageSize: 10,
          sort: "updatedAt:desc",
        },
        accessToken,
      ),
    placeholderData: keepPreviousData,
  });

  return (
    <div className="grid gap-6">
      <div className="page-header">
        <div>
          <p className="text-sm font-medium uppercase tracking-[0.2em] text-primary">Shipments</p>
          <h2 className="text-3xl font-semibold">Merchant shipment workspace</h2>
        </div>
        <p className="max-w-2xl text-sm text-muted-foreground">Backend hien chua co `GET /shipments` merchant list theo contract, nen route nay dung search API lam read model chinh de khong sua backend.</p>
      </div>

      <Card>
        <CardHeader>
          <CardTitle>Quick filters</CardTitle>
          <CardDescription>Search-backed filters cho tracking code, receiver phone va status.</CardDescription>
        </CardHeader>
        <CardContent className="grid gap-4 md:grid-cols-4">
          <div className="space-y-2">
            <Label htmlFor="tracking-filter">Tracking code</Label>
            <Input id="tracking-filter" value={trackingCode} onChange={(event) => setTrackingCode(event.target.value)} placeholder="LGA2603000001" />
          </div>
          <div className="space-y-2">
            <Label htmlFor="receiver-phone">Receiver phone</Label>
            <Input id="receiver-phone" value={receiverPhone} onChange={(event) => setReceiverPhone(event.target.value)} placeholder="0909..." />
          </div>
          <div className="space-y-2">
            <Label htmlFor="status-filter">Status</Label>
            <select
              id="status-filter"
              className="flex h-10 w-full rounded-md border border-input bg-background px-3 py-2 text-sm"
              value={status}
              onChange={(event) => setStatus(event.target.value as ShipmentStatus | "")}
            >
              <option value="">Tat ca</option>
              {shipmentStatusOptions.map((item) => (
                <option key={item} value={item}>
                  {item}
                </option>
              ))}
            </select>
          </div>
          <div className="flex items-end">
            <Button type="button" variant="outline" onClick={() => {
              setTrackingCode("");
              setReceiverPhone("");
              setStatus("");
            }}>
              Reset filters
            </Button>
          </div>
        </CardContent>
      </Card>

      {query.isLoading ? <PageLoadingState /> : null}
      {query.isError ? (
        <ErrorState description={query.error instanceof Error ? query.error.message : "Khong the tai shipments."} onRetry={() => query.refetch()} />
      ) : null}
      {query.isSuccess && query.data.items.length === 0 ? (
        <EmptyState icon={PackageSearch} title="Khong co ket qua" description="Thu bo bot filter hoac doi backend search indexing them du lieu." />
      ) : null}
      {query.isSuccess && query.data.items.length > 0 ? (
        <Card>
          <CardHeader>
            <CardTitle>Shipment list</CardTitle>
            <CardDescription>{query.data.total.toLocaleString("vi-VN")} ket qua.</CardDescription>
          </CardHeader>
          <CardContent className="overflow-x-auto">
            <table className="min-w-full text-left text-sm">
              <thead className="text-muted-foreground">
                <tr>
                  <th className="pb-3 font-medium">Tracking</th>
                  <th className="pb-3 font-medium">Shipment code</th>
                  <th className="pb-3 font-medium">Merchant</th>
                  <th className="pb-3 font-medium">Receiver</th>
                  <th className="pb-3 font-medium">Status</th>
                  <th className="pb-3 font-medium">COD</th>
                  <th className="pb-3 font-medium">Updated</th>
                </tr>
              </thead>
              <tbody>
                {query.data.items.map((item) => (
                  <tr key={item.shipmentId} className="border-t border-border/70">
                    <td className="py-4 font-medium">{item.trackingCode}</td>
                    <td className="py-4">{item.shipmentCode}</td>
                    <td className="py-4">{item.merchantCode}</td>
                    <td className="py-4">
                      <div>{item.receiverName}</div>
                      <div className="text-xs text-muted-foreground">{item.receiverPhone}</div>
                    </td>
                    <td className="py-4"><Badge variant={shipmentStatusTone[item.status]}>{item.status}</Badge></td>
                    <td className="py-4">{formatCurrency(item.codAmount)}</td>
                    <td className="py-4 text-muted-foreground">{formatDate(item.updatedAt)}</td>
                  </tr>
                ))}
              </tbody>
            </table>
          </CardContent>
        </Card>
      ) : null}
    </div>
  );
}