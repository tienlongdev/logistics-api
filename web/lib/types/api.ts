export type ShipmentStatus =
  | "Created"
  | "AwaitingPickup"
  | "PickedUp"
  | "InboundAtOriginHub"
  | "InTransit"
  | "InboundAtDestinationHub"
  | "OutForDelivery"
  | "DeliveryAttemptFailed"
  | "Delivered"
  | "Cancelled"
  | "Returned"
  | "ReturnInTransit"
  | "ReturnedToSender";

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
  serviceType: string;
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

export interface ProblemDetails {
  title?: string;
  detail?: string;
  status?: number;
  traceId?: string;
  correlationId?: string;
}

export interface DashboardSnapshot {
  items: SearchShipmentItem[];
  total: number;
  delivered: number;
  inTransit: number;
  cancelled: number;
}