"use client";

import { zodResolver } from "@hookform/resolvers/zod";
import { keepPreviousData, useQuery } from "@tanstack/react-query";
import { Filter, Search } from "lucide-react";
import { useState } from "react";
import { useForm } from "react-hook-form";

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
  const accessToken = useAuthStore((state) => state.accessToken ?? undefined);
  const [filters, setFilters] = useState<SearchFiltersSchema>(defaultFilters);
  const form = useForm<SearchFiltersSchema>({
    resolver: zodResolver(searchFiltersSchema),
    defaultValues: defaultFilters,
  });

  const query = useQuery({
    queryKey: ["search-shipments", filters, accessToken],
    queryFn: () => searchShipments({ ...filters, page: 1, pageSize: 20 }, accessToken),
    placeholderData: keepPreviousData,
  });

  return (
    <div className="grid gap-6">
      <div className="page-header">
        <div>
          <p className="text-sm font-medium uppercase tracking-[0.2em] text-primary">Search</p>
          <h2 className="text-3xl font-semibold">Elasticsearch-powered search</h2>
        </div>
        <p className="max-w-xl text-sm text-muted-foreground">Trang nay align voi filter set da ghi trong contract: trackingCode, shipmentCode, merchantCode, receiverPhone, status va date range.</p>
      </div>

      <Card>
        <CardHeader>
          <CardTitle>Advanced filters</CardTitle>
          <CardDescription>Form dung react-hook-form + zod de validate va serialise query params.</CardDescription>
        </CardHeader>
        <CardContent>
          <form className="grid gap-4 md:grid-cols-2 xl:grid-cols-4" onSubmit={form.handleSubmit((values) => setFilters(values))}>
            <div className="space-y-2">
              <Label htmlFor="search-tracking">Tracking code</Label>
              <Input id="search-tracking" {...form.register("trackingCode")} placeholder="LGA..." />
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

      {query.isLoading ? <PageLoadingState /> : null}

      {query.isSuccess && query.data.items.length === 0 ? (
        <EmptyState icon={Search} title="Khong co du lieu khop" description="Thu no wider date range hoac bo bot filter merchant / phone / status." />
      ) : null}

      {query.isSuccess && query.data.items.length > 0 ? (
        <Card>
          <CardHeader>
            <CardTitle>Search results</CardTitle>
            <CardDescription>{query.data.total.toLocaleString("vi-VN")} ket qua tu Elasticsearch read model.</CardDescription>
          </CardHeader>
          <CardContent className="grid gap-3">
            {query.data.items.map((item) => (
              <div key={item.shipmentId} className="rounded-2xl border border-border/70 bg-background/70 p-4">
                <div className="flex flex-col gap-3 md:flex-row md:items-center md:justify-between">
                  <div>
                    <div className="flex flex-wrap items-center gap-2">
                      <p className="font-medium">{item.trackingCode}</p>
                      <Badge variant={shipmentStatusTone[item.status]}>{item.status}</Badge>
                    </div>
                    <p className="text-sm text-muted-foreground">{item.shipmentCode} · {item.receiverName} · {item.receiverPhone}</p>
                  </div>
                  <div className="text-sm text-muted-foreground">Updated {formatDate(item.updatedAt)}</div>
                </div>
              </div>
            ))}
          </CardContent>
        </Card>
      ) : null}
    </div>
  );
}