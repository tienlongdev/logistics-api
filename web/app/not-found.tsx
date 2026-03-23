import Link from "next/link";

import { Button } from "@/components/ui/button";
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "@/components/ui/card";

export default function NotFound() {
  return (
    <div className="page-shell flex min-h-screen items-center justify-center">
      <Card className="max-w-lg">
        <CardHeader>
          <CardTitle>Khong tim thay trang</CardTitle>
          <CardDescription>Route nay chua duoc dinh nghia trong web app.</CardDescription>
        </CardHeader>
        <CardContent>
          <Button asChild>
            <Link href="/dashboard">Ve dashboard</Link>
          </Button>
        </CardContent>
      </Card>
    </div>
  );
}