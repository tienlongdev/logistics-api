import { Compass, Search } from "lucide-react";
import Link from "next/link";

import { Button } from "@/components/ui/button";
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "@/components/ui/card";

export default function NotFound() {
  return (
    <div className="page-shell flex min-h-screen items-center justify-center">
      <Card className="max-w-xl">
        <CardHeader>
          <div className="mb-4 flex h-12 w-12 items-center justify-center rounded-2xl bg-accent text-accent-foreground">
            <Compass className="h-6 w-6" />
          </div>
          <CardTitle>Không tìm thấy trang</CardTitle>
          <CardDescription>Trang bạn đang tìm không tồn tại hoặc đã được dời đi. Vui lòng quay về trang chủ.</CardDescription>
        </CardHeader>
        <CardContent className="flex flex-col gap-3 sm:flex-row">
          <Button asChild>
            <Link href="/dashboard">Quay về trang chủ</Link>
          </Button>
          <Button asChild variant="outline">
            <Link href="/search">
              <Search className="h-4 w-4" />
              Tìm kiếm đơn hàng
            </Link>
          </Button>
        </CardContent>
      </Card>
    </div>
  );
}