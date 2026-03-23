export const apiRoles = ["Admin", "Operator", "HubStaff", "Merchant"] as const;

export type ApiRole = (typeof apiRoles)[number];

export const shipmentStatuses = [
  "Created",
  "AwaitingPickup",
  "PickedUp",
  "InTransit",
  "OutForDelivery",
  "Delivered",
  "DeliveryFailed",
  "Returning",
  "Returned",
  "Cancelled",
] as const;

export type ShipmentStatus = (typeof shipmentStatuses)[number];

export const serviceTypes = ["Standard", "Express"] as const;

export type ServiceType = (typeof serviceTypes)[number];

export interface PaginatedResponse<TItem> {
  items: TItem[];
  page: number;
  pageSize: number;
  total: number;
}

export interface LoginRequest {
  email: string;
  password: string;
}

export interface LoginResponse {
  accessToken: string;
  refreshToken: string;
  expiresIn: number;
}

export interface SearchShipmentItem {
  shipmentId: string;
  trackingCode: string;
  shipmentCode: string;
  merchantCode: string;
  receiverPhone: string;
  receiverName: string;
  senderName: string;
  status: ShipmentStatus;
  serviceType: ServiceType;
  codAmount: number;
  shippingFee: number;
  totalFee: number;
  createdAt: string;
  updatedAt: string;
}

export interface SearchShipmentsResponse {
  total: number;
  page: number;
  pageSize: number;
  sort: string;
  items: SearchShipmentItem[];
}

export interface SearchShipmentFilters {
  trackingCode?: string;
  shipmentCode?: string;
  merchantCode?: string;
  receiverPhone?: string;
  status?: ShipmentStatus | "";
  fromDate?: string;
  toDate?: string;
  page?: number;
  pageSize?: number;
  sort?: string;
}

export interface TrackingSummary {
  trackingCode: string;
  shipmentCode: string;
  currentStatus: ShipmentStatus;
  receiverName: string;
  lastUpdatedAt: string;
}

export interface TrackingEvent {
  eventId: string;
  fromStatus: ShipmentStatus | null;
  toStatus: ShipmentStatus;
  hubCode: string | null;
  location: string | null;
  note: string | null;
  occurredAt: string;
}

export interface TrackingTimelineResponse {
  trackingCode: string;
  events: TrackingEvent[];
}

export interface TransitionShipmentStatusRequest {
  toStatus: ShipmentStatus;
  hubId?: string | null;
  hubCode?: string | null;
  location?: string | null;
  note?: string | null;
  occurredAt?: string | null;
}

export interface TransitionShipmentStatusResponse {
  shipmentId: string;
  trackingCode: string;
  fromStatus: ShipmentStatus;
  toStatus: ShipmentStatus;
  occurredAt: string;
  currentStatus: ShipmentStatus;
}

export interface ProblemDetails {
  title?: string;
  detail?: string;
  status?: number;
  traceId?: string;
  correlationId?: string;
  errors?: Record<string, string[]>;
}

export interface DashboardSnapshot {
  items: SearchShipmentItem[];
  total: number;
  delivered: number;
  inTransit: number;
  cancelled: number;
}