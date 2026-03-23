import type { ShipmentStatus } from "@/lib/types/api";

export const shipmentStatusTone: Record<ShipmentStatus, "default" | "secondary" | "success" | "destructive"> = {
  Created: "default",
  AwaitingPickup: "secondary",
  PickedUp: "default",
  InboundAtOriginHub: "secondary",
  InTransit: "default",
  InboundAtDestinationHub: "secondary",
  OutForDelivery: "default",
  DeliveryAttemptFailed: "destructive",
  Delivered: "success",
  Cancelled: "destructive",
  Returned: "destructive",
  ReturnInTransit: "destructive",
  ReturnedToSender: "destructive",
};

export const shipmentStatusOptions: ShipmentStatus[] = [
  "Created",
  "AwaitingPickup",
  "PickedUp",
  "InboundAtOriginHub",
  "InTransit",
  "InboundAtDestinationHub",
  "OutForDelivery",
  "DeliveryAttemptFailed",
  "Delivered",
  "Cancelled",
  "Returned",
  "ReturnInTransit",
  "ReturnedToSender",
];