"use client";

import { zodResolver } from "@hookform/resolvers/zod";
import { keepPreviousData, useQuery } from "@tanstack/react-query";
import { Filter, Search } from "lucide-react";
import Link from "next/link";
import { useSearchParams } from "next/navigation";
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
import { searchShipments } from "@/lib/api/client";
import { shipmentStatusOptions } from "@/lib/constants/status";
import { useAuthStore } from "@/lib/store/auth-store";
import { formatDate } from "@/lib/utils";
import { searchFiltersSchema, type SearchFiltersSchema } from "@/lib/validation/search";

const defaultFilters: SearchFiltersSchema = {
  trackingCode: "",
  shipmentCode: "",
  merchantCode: "",
  receiverPhone: "",
  status: "",
  fromDate: "",
  toDate: "",
  sort: "updatedAt:desc",
};

export default function SearchPage() {
  const searchParams = useSearchParams();
  const accessToken = useAuthStore((state) => state.accessToken ?? undefined);
  const trackingInputRef = useRef<HTMLInputElement | null>(null);
  const [filters, setFilters] = useState<SearchFiltersSchema>(defaultFilters);
  const [page, setPage] = useState(1);
  const [selectedShipmentId, setSelectedShipmentId] = useState<string | null>(null);
  const form = useForm<SearchFiltersSchema>({
    resolver: zodResolver(searchFiltersSchema),
    defaultValues: defaultFilters,
  });
  const trackingField = form.register("trackingCode");

  const query = useQuery({
    enabled: Boolean(accessToken),
    queryKey: ["search-shipments", filters, page, accessToken],
    queryFn: () => searchShipments({ ...filters, page, pageSize: 20 }, accessToken),
    placeholderData: keepPreviousData,
  });
  const selectedItem = query.data?.items.find((item) => item.shipmentId === selectedShipmentId) ?? query.data?.items[0] ?? null;
  const totalPages = query.data ? Math.max(1, Math.ceil(query.data.total / query.data.pageSize)) : 1;

  useEffect(() => {
    if (searchParams.get("focus") === "primary") {
      trackingInputRef.current?.focus();
    }
  }, [searchParams]);

  return (
    <div className="grid gap-6">
      <div className="page-header">
        <div className="space-y-3">
          <p className="section-kicker">Search</p>
          <h2 className="text-3xl font-semibold sm:text-4xl">Elasticsearch-powered search</h2>
          <p className="page-copy">Advanced filters stay within the existing search endpoint and are optimized for fast keyboard entry. Use the shell shortcut to land here and focus the first field instantly.</p>
        </div>
        <div className="keyboard-hint">Cmd/Ctrl + K</div>
      </div>

      <Card>
        <CardHeader>
          <CardTitle>Advanced filters</CardTitle>
          <CardDescription>Form dung react-hook-form + zod de validate va serialise query params.</CardDescription>
        </CardHeader>
        <CardContent>
          <form className="grid gap-4 md:grid-cols-2 xl:grid-cols-4" onSubmit={form.handleSubmit((values) => {
            setPage(1);
            setSelectedShipmentId(null);
            setFilters(values);
          })}>
            <div className="space-y-2">
              <Label htmlFor="search-tracking">Tracking code</Label>
              <Input
                id="search-tracking"
                {...trackingField}
                ref={(node) => {
                  trackingField.ref(node);
                  trackingInputRef.current = node;
                }}
                placeholder="LGA..."
              />
            </div>
            <div className="space-y-2">
              <Label htmlFor="search-shipment">Shipment code</Label>
              <Input id="search-shipment" {...form.register("shipmentCode")} placeholder="SHIP-..." />
            </div>
            <div className="space-y-2">
              <Label htmlFor="search-merchant">Merchant code</Label>
              <Input id="search-merchant" {...form.register("merchantCode")} placeholder="MERCHANT-ACME" />
            </div>
            <div className="space-y-2">
              <Label htmlFor="search-phone">Receiver phone</Label>
              <Input id="search-phone" {...form.register("receiverPhone")} placeholder="0909..." />
            </div>
            <div className="space-y-2">
              <Label htmlFor="search-status">Status</Label>
              <select id="search-status" className="flex h-10 w-full rounded-md border border-input bg-background px-3 py-2 text-sm" {...form.register("status")}>
                <option value="">Tat ca</option>
                {shipmentStatusOptions.map((item) => (
                  <option key={item} value={item}>{item}</option>
                ))}
              </select>
            </div>
            <div className="space-y-2">
              <Label htmlFor="search-from">From date</Label>
              <Input id="search-from" type="date" {...form.register("fromDate")} />
            </div>
            <div className="space-y-2">
              <Label htmlFor="search-to">To date</Label>
              <Input id="search-to" type="date" {...form.register("toDate")} />
              {form.formState.errors.toDate ? <p className="text-sm text-destructive">{form.formState.errors.toDate.message}</p> : null}
            </div>
            <div className="space-y-2">
              <Label htmlFor="search-sort">Sort</Label>
              <select id="search-sort" className="flex h-10 w-full rounded-md border border-input bg-background px-3 py-2 text-sm" {...form.register("sort")}>
                <option value="updatedAt:desc">updatedAt desc</option>
                <option value="createdAt:desc">createdAt desc</option>
                <option value="trackingCode:asc">trackingCode asc</option>
              </select>
            </div>
            <div className="flex items-end gap-2 xl:col-span-4">
              <Button type="submit">
                <Filter className="h-4 w-4" />
                Apply filters
              </Button>
              <Button type="button" variant="outline" onClick={() => {
                form.reset(defaultFilters);
                setFilters(defaultFilters);
                setPage(1);
                setSelectedShipmentId(null);
              }}>
                Reset
              </Button>
            </div>
          </form>
        </CardContent>
      </Card>

      {query.isError ? (
        <ErrorState description={query.error instanceof Error ? query.error.message : "Khong the tim kiem shipments."} onRetry={() => query.refetch()} />
      ) : null}

      {query.isLoading ? <PageLoadingState variant="list" /> : null}

      {query.isSuccess && query.data.items.length === 0 ? (
        <EmptyState icon={Search} title="Khong co du lieu khop" description="Thu no wider date range hoac bo bot filter merchant / phone / status." variant="search" />
      ) : null}

      {query.isSuccess && query.data.items.length > 0 ? (
        <div className="grid gap-4 xl:grid-cols-[1.1fr_0.9fr]">
          <Card>
            <CardHeader>
              <CardTitle>Search results</CardTitle>
              <CardDescription>{query.data.total.toLocaleString("vi-VN")} ket qua tu Elasticsearch read model.</CardDescription>
            </CardHeader>
            <CardContent className="grid gap-3">
              {query.data.items.map((item) => (
                <button
                  key={item.shipmentId}
                  type="button"
                  onClick={() => setSelectedShipmentId(item.shipmentId)}
                  className="rounded-2xl border border-border/70 bg-background/70 p-4 text-left transition-colors hover:border-primary/50"
                >
                  <div className="flex flex-col gap-3 md:flex-row md:items-center md:justify-between">
                    <div>
                      <div className="flex flex-wrap items-center gap-2">
                        <p className="font-medium">{item.trackingCode}</p>
                        <StatusBadge status={item.status} />
                      </div>
                      <p className="text-sm text-muted-foreground">{item.shipmentCode} · {item.receiverName} · {item.receiverPhone}</p>
                    </div>
                    <div className="text-sm text-muted-foreground">Updated {formatDate(item.updatedAt)}</div>
                  </div>
                </button>
              ))}
              <div className="flex flex-col gap-3 border-t border-border/70 pt-4 text-sm sm:flex-row sm:items-center sm:justify-between">
                <div className="text-muted-foreground">Trang {page} / {totalPages}</div>
                <div className="flex items-center gap-2">
                  <Button variant="outline" onClick={() => setPage((current) => Math.max(1, current - 1))} disabled={page === 1 || query.isFetching}>Trang truoc</Button>
                  <Button variant="outline" onClick={() => setPage((current) => Math.min(totalPages, current + 1))} disabled={page >= totalPages || query.isFetching}>Trang sau</Button>
                </div>
              </div>
            </CardContent>
          </Card>

          <Card>
            <CardHeader>
              <CardTitle>Preview</CardTitle>
              <CardDescription>Xem nhanh shipment da chon truoc khi mo detail page.</CardDescription>
            </CardHeader>
            <CardContent className="space-y-4 text-sm">
              {selectedItem ? (
                <>
                  <div className="space-y-2 rounded-2xl bg-accent/60 p-4">
                    <div className="flex flex-wrap items-center gap-2">
                      <p className="text-lg font-semibold">{selectedItem.trackingCode}</p>
                      <StatusBadge status={selectedItem.status} />
                    </div>
                    <p className="text-muted-foreground">{selectedItem.shipmentCode}</p>
                  </div>
                  <div className="grid gap-3 sm:grid-cols-2">
                    <div>
                      <p className="text-xs uppercase tracking-[0.16em] text-muted-foreground">Receiver</p>
                      <p className="mt-1 font-medium">{selectedItem.receiverName}</p>
                      <p className="text-muted-foreground">{selectedItem.receiverPhone}</p>
                    </div>
                    <div>
                      <p className="text-xs uppercase tracking-[0.16em] text-muted-foreground">Sender</p>
                      <p className="mt-1 font-medium">{selectedItem.senderName}</p>
                      <p className="text-muted-foreground">{selectedItem.merchantCode}</p>
                    </div>
                  </div>
                  <div className="grid gap-3 sm:grid-cols-2">
                    <div>
                      <p className="text-xs uppercase tracking-[0.16em] text-muted-foreground">Created</p>
                      <p className="mt-1 font-medium">{formatDate(selectedItem.createdAt)}</p>
                    </div>
                    <div>
                      <p className="text-xs uppercase tracking-[0.16em] text-muted-foreground">Updated</p>
                      <p className="mt-1 font-medium">{formatDate(selectedItem.updatedAt)}</p>
                    </div>
                  </div>
                  <div className="grid gap-3 sm:grid-cols-3">
                    <div>
                      <p className="text-xs uppercase tracking-[0.16em] text-muted-foreground">COD</p>
                      <p className="mt-1 font-medium">{selectedItem.codAmount.toLocaleString("vi-VN")}</p>
                    </div>
                    <div>
                      <p className="text-xs uppercase tracking-[0.16em] text-muted-foreground">Shipping fee</p>
                      <p className="mt-1 font-medium">{selectedItem.shippingFee.toLocaleString("vi-VN")}</p>
                    </div>
                    <div>
                      <p className="text-xs uppercase tracking-[0.16em] text-muted-foreground">Total fee</p>
                      <p className="mt-1 font-medium">{selectedItem.totalFee.toLocaleString("vi-VN")}</p>
                    </div>
                  </div>
                  <Button asChild className="w-full">
                    <Link
                      href={{
                        pathname: `/shipments/${selectedItem.shipmentId}`,
                        query: {
                          codAmount: String(selectedItem.codAmount),
                          createdAt: selectedItem.createdAt,
                          receiverName: selectedItem.receiverName,
                          receiverPhone: selectedItem.receiverPhone,
                          senderName: selectedItem.senderName,
                          shipmentCode: selectedItem.shipmentCode,
                          shippingFee: String(selectedItem.shippingFee),
                          status: selectedItem.status,
                          totalFee: String(selectedItem.totalFee),
                          trackingCode: selectedItem.trackingCode,
                        },
                      }}
                    >
                      Mo shipment detail
                    </Link>
                  </Button>
                </>
              ) : (
                <EmptyState icon={Search} title="Chua chon shipment" description="Chon mot ket qua o cot ben trai de xem preview chi tiet." />
              )}
            </CardContent>
          </Card>
        </div>
      ) : null}
    </div>
  );
}