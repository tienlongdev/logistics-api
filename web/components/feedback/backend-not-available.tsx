import { AlertTriangle } from "lucide-react";

import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "@/components/ui/card";

interface BackendNotAvailableProps {
  description: string;
  title?: string;
  todo?: string;
}

export function BackendNotAvailable({
  description,
  title = "Tính năng đang phát triển",
  todo,
}: BackendNotAvailableProps) {
  return (
    <Card className="border-amber-300/70 bg-amber-50/80 shadow-none dark:border-amber-700/60 dark:bg-amber-950/30">
      <CardHeader>
        <CardTitle className="flex items-center gap-2 text-base text-amber-950 dark:text-amber-100">
          <AlertTriangle className="h-4 w-4" />
          {title}
        </CardTitle>
        <CardDescription className="text-amber-900/80 dark:text-amber-200/80">{description}</CardDescription>
      </CardHeader>
      {todo ? (
        <CardContent>
          <p className="text-sm text-amber-950 dark:text-amber-100">{todo}</p>
        </CardContent>
      ) : null}
    </Card>
  );
}