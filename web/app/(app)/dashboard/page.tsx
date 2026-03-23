"use client";

import { useQuery } from "@tanstack/react-query";
import { BarChart3, CircleAlert, Package, Truck } from "lucide-react";

import { EmptyState } from "@/components/feedback/empty-state";
import { ErrorState } from "@/components/feedback/error-state";
import { PageLoadingState } from "@/components/feedback/page-loading-state";
import { Badge } from "@/components/ui/badge";
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "@/components/ui/card";
import { getDashboardSnapshot } from "@/lib/api/client";
import { shipmentStatusTone } from "@/lib/constants/status";
import { useAuthStore } from "@/lib/store/auth-store";
import { formatCurrency, formatDate } from "@/lib/utils";

const statCards = [
  {
    key: "total",
    label: "Tong shipments",
    icon: Package,
  },
  {
    key: "inTransit",
    label: "Dang van chuyen",
    icon: Truck,
  },
  {
    key: "delivered",
    label: "Da giao",
    icon: BarChart3,
  },
  {
    key: "cancelled",
    label: "Da huy",
    icon: CircleAlert,
  },
] as const;

export default function DashboardPage() {
  const accessToken = useAuthStore((state) => state.accessToken ?? undefined);
  const query = useQuery({
    queryKey: ["dashboard", accessToken],
    queryFn: () => getDashboardSnapshot(accessToken),
  });

  return (
    <div className="grid gap-6">
      <div className="page-header">
        <div>
          <p className="text-sm font-medium uppercase tracking-[0.2em] text-primary">Overview</p>
          <h2 className="text-3xl font-semibold">Operational pulse</h2>
        </div>
        <p className="max-w-xl text-sm text-muted-foreground">Dashboard hien lay du lieu tu search endpoint de khong can them backend moi. Neu search API chua san sang, app se hien mock fallback de kiem thu shell va states.</p>
      </div>

      {query.isLoading ? <PageLoadingState /> : null}

      {query.isError ? (
        <ErrorState description={query.error instanceof Error ? query.error.message : "Khong the tai dashboard."} onRetry={() => query.refetch()} />
      ) : null}

      {query.isSuccess ? (
        <div className="grid gap-6">
          <section className="grid-stagger grid gap-4 md:grid-cols-2 xl:grid-cols-4">
            {statCards.map(({ key, label, icon: Icon }) => (
              <Card key={key}>
                <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-3">
                  <CardDescription>{label}</CardDescription>
                  <div className="rounded-full bg-accent p-2 text-accent-foreground">
                    <Icon className="h-4 w-4" />
                  </div>
                </CardHeader>
                <CardContent>
                  <div className="text-3xl font-semibold">{query.data[key].toLocaleString("vi-VN")}</div>
                </CardContent>
              </Card>
            ))}
          </section>

          {query.data.items.length === 0 ? (
            <EmptyState
              icon={Package}
              title="Chua co shipment gan day"
              description="Khi backend co du lieu search, dashboard se tu dong render bang recent shipments tai day."
            />
          ) : (
            <Card>
              <CardHeader>
                <CardTitle>Recent shipments</CardTitle>
                <CardDescription>Du lieu lay tu `GET /api/v1/search/shipments`.</CardDescription>
              </CardHeader>
              <CardContent className="overflow-x-auto">
                <table className="min-w-full text-left text-sm">
                  <thead className="text-muted-foreground">
                    <tr>
                      <th className="pb-3 font-medium">Tracking</th>
                      <th className="pb-3 font-medium">Receiver</th>
                      <th className="pb-3 font-medium">Status</th>
                      <th className="pb-3 font-medium">Total fee</th>
                      <th className="pb-3 font-medium">Updated</th>
                    </tr>
                  </thead>
                  <tbody>
                    {query.data.items.map((item) => (
                      <tr key={item.shipmentId} className="border-t border-border/70">
                        <td className="py-4 font-medium">{item.trackingCode}</td>
                        <td className="py-4">{item.receiverName}</td>
                        <td className="py-4">
                          <Badge variant={shipmentStatusTone[item.status]}>{item.status}</Badge>
                        </td>
                        <td className="py-4">{formatCurrency(item.totalFee)}</td>
                        <td className="py-4 text-muted-foreground">{formatDate(item.updatedAt)}</td>
                      </tr>
                    ))}
                  </tbody>
                </table>
              </CardContent>
            </Card>
          )}
        </div>
      ) : null}
    </div>
  );
}