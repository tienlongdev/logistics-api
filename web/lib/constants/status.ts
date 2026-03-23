import { shipmentStatuses, type ShipmentStatus } from "@/lib/types/api";

export const shipmentStatusTone: Record<ShipmentStatus, "default" | "secondary" | "success" | "destructive"> = {
  Created: "default",
  AwaitingPickup: "secondary",
  PickedUp: "default",
  InTransit: "default",
  OutForDelivery: "default",
  DeliveryFailed: "destructive",
  Delivered: "success",
  Returning: "secondary",
  Cancelled: "destructive",
  Returned: "destructive",
};

export const shipmentStatusOptions = [...shipmentStatuses];

export const shipmentStatusValueMap: Record<ShipmentStatus, number> = {
  Created: 1,
  AwaitingPickup: 2,
  PickedUp: 3,
  InTransit: 4,
  OutForDelivery: 5,
  Delivered: 6,
  DeliveryFailed: 7,
  Returning: 8,
  Returned: 9,
  Cancelled: 10,
};

export const privilegedRoles = new Set(["Admin", "Operator", "HubStaff"]);