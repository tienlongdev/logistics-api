"use client";

import { zodResolver } from "@hookform/resolvers/zod";
import { keepPreviousData, useQuery } from "@tanstack/react-query";
import { Boxes, PackageSearch, Search } from "lucide-react";
import Link from "next/link";
import { useSearchParams } from "next/navigation";
import { useEffect, useRef, useState } from "react";
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
  const searchParams = useSearchParams();
  const accessToken = useAuthStore((state) => state.accessToken ?? undefined);
  const codeInputRef = useRef<HTMLInputElement | null>(null);
  const form = useForm<ShipmentFiltersSchema>({
    resolver: zodResolver(shipmentFiltersSchema),
    defaultValues: defaultFilters,
  });
  const codeField = form.register("code");
  const [filters, setFilters] = useState<ShipmentFiltersSchema>(defaultFilters);
  const [page, setPage] = useState(1);

  useEffect(() => {
    if (searchParams.get("focus") === "code") {
      codeInputRef.current?.focus();
    }
  }, [searchParams]);

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
        <div className="space-y-3">
          <p className="section-kicker">Shipments</p>
          <h2 className="text-3xl font-semibold sm:text-4xl">Merchant shipment workspace</h2>
          <p className="page-copy">This screen keeps using the implemented search endpoint as the list read model until dedicated merchant shipment listing APIs are available.</p>
        </div>
        <div className="flex items-center gap-3">
          <Button variant="outline" onClick={() => codeInputRef.current?.focus()}>
            <Search className="h-4 w-4" />
            Focus code filter
          </Button>
        </div>
      </div>

      <BackendNotAvailable
        description="Dedicated shipment list/detail/cancel APIs are still missing in backend. UI nay dung search endpoint cho danh sach va tracking endpoints cho timeline."
        todo="TODO: docs/tasks.md items E5 and E3 can bo sung `GET /api/v1/shipments`, `GET /api/v1/shipments/{id}`, `GET /api/v1/shipments/by-tracking/{trackingCode}`, va `POST /api/v1/shipments/{id}/cancel`."
      />

      <Card>
        <CardHeader>
          <CardTitle>Quick filters</CardTitle>
          <CardDescription>Filter by tracking or shipment code, receiver phone, date range, and status on the current backend search contract.</CardDescription>
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
              <Input
                id="code-filter"
                placeholder="Tracking code hoac shipment code"
                {...codeField}
                ref={(node) => {
                  codeField.ref(node);
                  codeInputRef.current = node;
                }}
              />
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

      {query.isLoading ? <PageLoadingState variant="list" /> : null}
      {query.isError ? (
        <ErrorState description={query.error instanceof Error ? query.error.message : "Khong the tai shipments."} onRetry={() => query.refetch()} />
      ) : null}
      {query.isSuccess && query.data.items.length === 0 ? (
        <EmptyState icon={PackageSearch} title="Khong co ket qua" description="Thu bo bot filter hoac doi backend search indexing them du lieu." variant="shipments" />
      ) : null}
      {query.isSuccess && query.data.items.length > 0 ? (
        <Card>
          <CardHeader>
            <CardTitle>Shipment list</CardTitle>
            <CardDescription>{query.data.total.toLocaleString("vi-VN")} ket qua. Desktop dung bang, mobile dung stacked cards de giu kha nang doc nhanh.</CardDescription>
          </CardHeader>
          <CardContent className="overflow-x-auto">
            <div className="grid gap-3 md:hidden">
              {query.data.items.map((item) => (
                <Link
                  key={item.shipmentId}
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
                  className="rounded-[1.2rem] border border-border/70 bg-background/70 p-4 transition-colors hover:border-primary/40"
                >
                  <div className="flex items-start justify-between gap-3">
                    <div>
                      <p className="font-semibold text-primary">{item.trackingCode}</p>
                      <p className="mt-1 text-sm text-muted-foreground">{item.shipmentCode}</p>
                    </div>
                    <StatusBadge status={item.status} />
                  </div>
                  <div className="mt-4 space-y-3 text-sm">
                    <div>
                      <p className="font-medium">{item.receiverName}</p>
                      <p className="text-muted-foreground">{item.receiverPhone}</p>
                    </div>
                    <div className="grid grid-cols-3 gap-3">
                      <div>
                        <p className="text-xs uppercase tracking-[0.18em] text-muted-foreground">Created</p>
                        <p className="mt-1 font-medium">{formatDate(item.createdAt)}</p>
                      </div>
                      <div>
                        <p className="text-xs uppercase tracking-[0.18em] text-muted-foreground">COD</p>
                        <p className="mt-1 font-medium">{formatCurrency(item.codAmount)}</p>
                      </div>
                      <div>
                        <p className="text-xs uppercase tracking-[0.18em] text-muted-foreground">Fee</p>
                        <p className="mt-1 font-medium">{formatCurrency(item.totalFee)}</p>
                      </div>
                    </div>
                  </div>
                </Link>
              ))}
            </div>
            <table className="data-table hidden md:table">
              <thead>
                <tr>
                  <th>Tracking</th>
                  <th>Shipment code</th>
                  <th>Receiver</th>
                  <th>Status</th>
                  <th>Created</th>
                  <th>COD</th>
                  <th>Fee</th>
                </tr>
              </thead>
              <tbody>
                {query.data.items.map((item) => (
                  <tr key={item.shipmentId}>
                    <td className="font-medium">
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
                    <td>{item.shipmentCode}</td>
                    <td>
                      <div>{item.receiverName}</div>
                      <div className="text-xs text-muted-foreground">{item.receiverPhone}</div>
                    </td>
                    <td><StatusBadge status={item.status} /></td>
                    <td className="text-muted-foreground">{formatDate(item.createdAt)}</td>
                    <td>{formatCurrency(item.codAmount)}</td>
                    <td>{formatCurrency(item.totalFee)}</td>
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