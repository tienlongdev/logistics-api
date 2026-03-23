import { Badge } from "@/components/ui/badge";
import { shipmentStatusLabel, shipmentStatusTone } from "@/lib/constants/status";
import type { ShipmentStatus } from "@/lib/types/api";

interface StatusBadgeProps {
  status: ShipmentStatus;
}

export function StatusBadge({ status }: StatusBadgeProps) {
  return <Badge variant={shipmentStatusTone[status]}>{shipmentStatusLabel[status] ?? status}</Badge>;
}