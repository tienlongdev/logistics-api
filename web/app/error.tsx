"use client";

import { AlertTriangle } from "lucide-react";

import { Button } from "@/components/ui/button";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";

export default function GlobalError({ reset }: { error: Error & { digest?: string }; reset: () => void }) {
  return (
    <div className="page-shell flex min-h-screen items-center justify-center">
      <Card className="max-w-lg">
        <CardHeader>
          <div className="mb-4 flex h-12 w-12 items-center justify-center rounded-full bg-destructive/10 text-destructive">
            <AlertTriangle className="h-6 w-6" />
          </div>
          <CardTitle>Ung dung gap loi khong mong muon</CardTitle>
        </CardHeader>
        <CardContent className="space-y-4">
          <p className="text-sm text-muted-foreground">
            Hay thu tai lai. Neu loi lap lai, kiem tra cau hinh API base URL trong bien moi truong frontend.
          </p>
          <Button onClick={reset}>Thu lai</Button>
        </CardContent>
      </Card>
    </div>
  );
}