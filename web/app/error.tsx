"use client";

import { AlertTriangle, RotateCcw } from "lucide-react";

import { Button } from "@/components/ui/button";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";

export default function GlobalError({ error, reset }: { error: Error & { digest?: string }; reset: () => void }) {
  return (
    <div className="page-shell flex min-h-screen items-center justify-center">
      <Card className="max-w-xl">
        <CardHeader>
          <div className="mb-4 flex h-12 w-12 items-center justify-center rounded-full bg-destructive/10 text-destructive">
            <AlertTriangle className="h-6 w-6" />
          </div>
          <CardTitle>Đã xảy ra lỗi</CardTitle>
        </CardHeader>
        <CardContent className="space-y-4">
          <p className="text-sm text-muted-foreground">
            Đã xảy ra lỗi không mong muốn. Vui lòng thử tải lại trang.
          </p>
          {error.digest ? <p className="text-xs text-muted-foreground">Mã lỗi: {error.digest}</p> : null}
          <Button onClick={reset}>
            <RotateCcw className="h-4 w-4" />
            Thử lại
          </Button>
        </CardContent>
      </Card>
    </div>
  );
}