"use client";

import { zodResolver } from "@hookform/resolvers/zod";
import { keepPreviousData, useQuery } from "@tanstack/react-query";
import { Boxes, PackageSearch } from "lucide-react";
import Link from "next/link";
import { useState } from "react";
import { useForm } from "react-hook-form";

import { BackendNotAvailable } from "@/components/feedback/backend-not-available";
import { EmptyState } from "@/components/feedback/empty-state";
import { ErrorState } from "@/components/feedback/error-state";
import { PageLoadingState } from "@/components/feedback/page-loading-state";
import { Button } from "@/components/ui/button";
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "@/components/ui/card";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { StatusBadge } from "@/components/ui/status-badge";
import { searchShipments } from "@/lib/api/client";
import { shipmentStatusOptions } from "@/lib/constants/status";
import { useAuthStore } from "@/lib/store/auth-store";
import { formatCurrency, formatDate, isUuid } from "@/lib/utils";
import { shipmentFiltersSchema, type ShipmentFiltersSchema } from "@/lib/validation/shipments";

const defaultFilters: ShipmentFiltersSchema = {
  code: "",
  fromDate: "",
  receiverPhone: "",
  status: "",
  toDate: "",
};

function mapCodeFilters(code: string) {
  const normalized = code.trim();

  if (!normalized) {
    return {
      shipmentCode: undefined,
      trackingCode: undefined,
    };
  }

  return normalized.toUpperCase().startsWith("LGA")
    ? { shipmentCode: undefined, trackingCode: normalized }
    : { shipmentCode: normalized, trackingCode: undefined };
}

export default function ShipmentsPage() {
  const accessToken = useAuthStore((state) => state.accessToken ?? undefined);
  const form = useForm<ShipmentFiltersSchema>({
    resolver: zodResolver(shipmentFiltersSchema),
    defaultValues: defaultFilters,
  });
  const [filters, setFilters] = useState<ShipmentFiltersSchema>(defaultFilters);
  const [page, setPage] = useState(1);

  const query = useQuery({
    enabled: Boolean(accessToken),
    queryKey: ["shipments", filters, page, accessToken],
    queryFn: () => {
      const codeFilters = mapCodeFilters(filters.code ?? "");

      return searchShipments(
        {
          ...codeFilters,
          fromDate: filters.fromDate,
          page,
          pageSize: 10,
          receiverPhone: filters.receiverPhone,
          sort: "updatedAt:desc",
          status: filters.status,
          toDate: filters.toDate,
        },
        accessToken,
      );
    },
    placeholderData: keepPreviousData,
  });

  const totalPages = query.data ? Math.max(1, Math.ceil(query.data.total / query.data.pageSize)) : 1;

  return (
    <div className="grid gap-6">
      <div className="page-header">
        <div>
          <p className="text-sm font-medium uppercase tracking-[0.2em] text-primary">Shipments</p>
          <h2 className="text-3xl font-semibold">Merchant shipment workspace</h2>
        </div>
        <p className="max-w-2xl text-sm text-muted-foreground">Table nay dung search read model vi backend hien chua co `GET /api/v1/shipments` merchant listing API.</p>
      </div>

      <BackendNotAvailable
        description="Dedicated shipment list/detail/cancel APIs are still missing in backend. UI nay dung search endpoint cho danh sach va tracking endpoints cho timeline."
        todo="TODO: docs/tasks.md items E5 and E3 can bo sung `GET /api/v1/shipments`, `GET /api/v1/shipments/{id}`, `GET /api/v1/shipments/by-tracking/{trackingCode}`, va `POST /api/v1/shipments/{id}/cancel`."
      />

      <Card>
        <CardHeader>
          <CardTitle>Quick filters</CardTitle>
          <CardDescription>Filter theo status, date range, code va receiver phone tren `GET /api/v1/search/shipments`.</CardDescription>
        </CardHeader>
        <CardContent>
          <form
            className="grid gap-4 md:grid-cols-2 xl:grid-cols-5"
            onSubmit={form.handleSubmit((values) => {
              setPage(1);
              setFilters(values);
            })}
          >
            <div className="space-y-2 xl:col-span-2">
              <Label htmlFor="code-filter">Code</Label>
              <Input id="code-filter" placeholder="Tracking code hoac shipment code" {...form.register("code")} />
            </div>
            <div className="space-y-2">
              <Label htmlFor="receiver-phone">Receiver phone</Label>
              <Input id="receiver-phone" placeholder="0909..." {...form.register("receiverPhone")} />
            </div>
            <div className="space-y-2">
              <Label htmlFor="status-filter">Status</Label>
              <select id="status-filter" className="flex h-10 w-full rounded-md border border-input bg-background px-3 py-2 text-sm" {...form.register("status")}>
                <option value="">Tat ca</option>
                {shipmentStatusOptions.map((item) => (
                  <option key={item} value={item}>{item}</option>
                ))}
              </select>
            </div>
            <div className="space-y-2">
              <Label htmlFor="from-date">From date</Label>
              <Input id="from-date" type="date" {...form.register("fromDate")} />
            </div>
            <div className="space-y-2">
              <Label htmlFor="to-date">To date</Label>
              <Input id="to-date" type="date" {...form.register("toDate")} />
              {form.formState.errors.toDate ? <p className="text-sm text-destructive">{form.formState.errors.toDate.message}</p> : null}
            </div>
            <div className="flex items-end gap-2 xl:col-span-5">
              <Button type="submit">Apply filters</Button>
              <Button type="button" variant="outline" onClick={() => {
                form.reset(defaultFilters);
                setFilters(defaultFilters);
                setPage(1);
              }}>
                Reset filters
              </Button>
            </div>
          </form>
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
                  <th className="pb-3 font-medium">Receiver</th>
                  <th className="pb-3 font-medium">Status</th>
                  <th className="pb-3 font-medium">Created</th>
                  <th className="pb-3 font-medium">COD</th>
                  <th className="pb-3 font-medium">Fee</th>
                </tr>
              </thead>
              <tbody>
                {query.data.items.map((item) => (
                  <tr key={item.shipmentId} className="border-t border-border/70">
                    <td className="py-4 font-medium">
                      <Link
                        href={{
                          pathname: `/shipments/${isUuid(item.shipmentId) ? item.shipmentId : item.trackingCode}`,
                          query: {
                            codAmount: String(item.codAmount),
                            createdAt: item.createdAt,
                            receiverName: item.receiverName,
                            receiverPhone: item.receiverPhone,
                            senderName: item.senderName,
                            shipmentCode: item.shipmentCode,
                            shippingFee: String(item.shippingFee),
                            status: item.status,
                            totalFee: String(item.totalFee),
                            trackingCode: item.trackingCode,
                          },
                        }}
                        className="text-primary underline-offset-4 hover:underline"
                      >
                        {item.trackingCode}
                      </Link>
                    </td>
                    <td className="py-4">{item.shipmentCode}</td>
                    <td className="py-4">
                      <div>{item.receiverName}</div>
                      <div className="text-xs text-muted-foreground">{item.receiverPhone}</div>
                    </td>
                    <td className="py-4"><StatusBadge status={item.status} /></td>
                    <td className="py-4 text-muted-foreground">{formatDate(item.createdAt)}</td>
                    <td className="py-4">{formatCurrency(item.codAmount)}</td>
                    <td className="py-4">{formatCurrency(item.totalFee)}</td>
                  </tr>
                ))}
              </tbody>
            </table>
            <div className="mt-4 flex flex-col gap-3 border-t border-border/70 pt-4 text-sm sm:flex-row sm:items-center sm:justify-between">
              <div className="text-muted-foreground">Trang {page} / {totalPages}</div>
              <div className="flex items-center gap-2">
                <Button variant="outline" onClick={() => setPage((current) => Math.max(1, current - 1))} disabled={page === 1 || query.isFetching}>Trang truoc</Button>
                <Button variant="outline" onClick={() => setPage((current) => Math.min(totalPages, current + 1))} disabled={page >= totalPages || query.isFetching}>Trang sau</Button>
              </div>
            </div>
          </CardContent>
        </Card>
      ) : null}

      {query.isSuccess && query.data.items.length > 0 ? (
        <Card>
          <CardHeader>
            <CardTitle className="flex items-center gap-2">
              <Boxes className="h-4 w-4 text-primary" />
              Detail availability
            </CardTitle>
            <CardDescription>
              Bam vao tracking code de mo shipment detail. Timeline se den tu tracking API, cac block package/sender se hien fallback cho den khi E5 xong.
            </CardDescription>
          </CardHeader>
        </Card>
      ) : null}
    </div>
  );
}