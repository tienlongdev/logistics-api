import { Badge } from "@/components/ui/badge";
import { shipmentStatusTone } from "@/lib/constants/status";
import type { ShipmentStatus } from "@/lib/types/api";

interface StatusBadgeProps {
  status: ShipmentStatus;
}

export function StatusBadge({ status }: StatusBadgeProps) {
  return <Badge variant={shipmentStatusTone[status]}>{status}</Badge>;
}