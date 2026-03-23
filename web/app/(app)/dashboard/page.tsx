"use client";

import { useQuery } from "@tanstack/react-query";
import { ArrowUpRight, BarChart3, CircleAlert, Package, Truck } from "lucide-react";
import Link from "next/link";

import { EmptyState } from "@/components/feedback/empty-state";
import { ErrorState } from "@/components/feedback/error-state";
import { PageLoadingState } from "@/components/feedback/page-loading-state";
import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "@/components/ui/card";
import { getDashboardSnapshot } from "@/lib/api/client";
import { shipmentStatusTone } from "@/lib/constants/status";
import { useAuthStore } from "@/lib/store/auth-store";
import { formatCurrency, formatDate } from "@/lib/utils";

const statCards = [
  {
    key: "total",
    label: "Tổng đơn hàng",
    icon: Package,
  },
  {
    key: "inTransit",
    label: "Đang vận chuyển",
    icon: Truck,
  },
  {
    key: "delivered",
    label: "Đã giao",
    icon: BarChart3,
  },
  {
    key: "cancelled",
    label: "Đã hủy",
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
        <div className="space-y-3">
          <p className="section-kicker">Tổng quan</p>
          <h2 className="text-3xl font-semibold sm:text-4xl">Hoạt động hôm nay</h2>
          <p className="page-copy">Theo dõi tổng hợp tình trạng đơn hàng và các giao dịch mới nhất.</p>
        </div>
        <div className="flex flex-wrap items-center gap-3">
          <Button asChild variant="outline">
            <Link href="/shipments">
              Xem danh sách đơn hàng
              <ArrowUpRight className="h-4 w-4" />
            </Link>
          </Button>
        </div>
      </div>

      {query.isLoading ? <PageLoadingState variant="dashboard" /> : null}

      {query.isError ? (
        <ErrorState description={query.error instanceof Error ? query.error.message : "Không thể tải dữ liệu. Vui lòng thử lại."} onRetry={() => query.refetch()} />
      ) : null}

      {query.isSuccess ? (
        <div className="grid gap-6">
          <section className="grid-stagger grid gap-4 md:grid-cols-2 xl:grid-cols-4">
            {statCards.map(({ key, label, icon: Icon }) => (
              <Card key={key} className="bg-gradient-to-br from-card via-card to-accent/20">
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
              title="Chưa có đơn hàng nào"
              description="Khi có dữ liệu, các đơn hàng mới nhất sẽ hiển thị tại đây."
              variant="shipments"
            />
          ) : (
            <Card>
              <CardHeader>
                <CardTitle>Đơn hàng gần đây</CardTitle>
                <CardDescription>Các đơn hàng mới nhất trong hệ thống.</CardDescription>
              </CardHeader>
              <CardContent className="overflow-x-auto">
                <div className="grid gap-3 md:hidden">
                  {query.data.items.map((item) => (
                    <div key={item.shipmentId} className="rounded-[1.2rem] border border-border/70 bg-background/70 p-4">
                      <div className="flex items-start justify-between gap-3">
                        <div>
                          <p className="font-semibold">{item.trackingCode}</p>
                          <p className="mt-1 text-sm text-muted-foreground">{item.receiverName}</p>
                        </div>
                        <Badge variant={shipmentStatusTone[item.status]}>{item.status}</Badge>
                      </div>
                      <div className="mt-4 grid grid-cols-2 gap-3 text-sm">
                        <div>
                        <p className="text-xs uppercase tracking-[0.18em] text-muted-foreground">Phí</p>
                          <p className="mt-1 font-medium">{formatCurrency(item.totalFee)}</p>
                        </div>
                      <div>
                        <p className="text-xs uppercase tracking-[0.18em] text-muted-foreground">Cập nhật</p>
                          <p className="mt-1 font-medium">{formatDate(item.updatedAt)}</p>
                        </div>
                      </div>
                    </div>
                  ))}
                </div>
                <table className="data-table hidden md:table">
                  <thead>
                    <tr>
                      <th>Mã vận đơn</th>
                      <th>Người nhận</th>
                      <th>Trạng thái</th>
                      <th>Phí giao</th>
                      <th>Cập nhật</th>
                    </tr>
                  </thead>
                  <tbody>
                    {query.data.items.map((item) => (
                      <tr key={item.shipmentId}>
                        <td className="font-medium">{item.trackingCode}</td>
                        <td>{item.receiverName}</td>
                        <td>
                          <Badge variant={shipmentStatusTone[item.status]}>{item.status}</Badge>
                        </td>
                        <td>{formatCurrency(item.totalFee)}</td>
                        <td className="text-muted-foreground">{formatDate(item.updatedAt)}</td>
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