import {
    type SearchShipmentsResponse,
    type TrackingSummary,
    type TrackingTimelineResponse,
} from "@/lib/types/api";

export const mockShipmentsResponse: SearchShipmentsResponse = {
  total: 4,
  page: 1,
  pageSize: 10,
  sort: "updatedAt:desc",
  items: [
    {
      shipmentId: "0f8fad5b-d9cb-469f-a165-70867728950e",
      trackingCode: "LGA2603000001",
      shipmentCode: "SHIP-20260323-0001",
      merchantCode: "MERCHANT-ACME",
      receiverPhone: "0909123456",
      receiverName: "Tran Thi B",
      senderName: "Nguyen Van A",
      status: "InTransit",
      serviceType: "Express",
      codAmount: 150000,
      shippingFee: 32000,
      totalFee: 32000,
      createdAt: "2026-03-23T10:00:00Z",
      updatedAt: "2026-03-23T11:05:00Z"
    },
    {
      shipmentId: "dd69e619-a1af-44b2-bf00-fdc5c086f912",
      trackingCode: "LGA2603000002",
      shipmentCode: "SHIP-20260323-0002",
      merchantCode: "MERCHANT-ACME",
      receiverPhone: "0912333444",
      receiverName: "Le Minh Chau",
      senderName: "Nguyen Van A",
      status: "Delivered",
      serviceType: "Standard",
      codAmount: 0,
      shippingFee: 24000,
      totalFee: 24000,
      createdAt: "2026-03-22T08:20:00Z",
      updatedAt: "2026-03-22T14:15:00Z"
    },
    {
      shipmentId: "cc3e5db8-5076-4dbe-a11c-964cfa4be661",
      trackingCode: "LGA2603000003",
      shipmentCode: "SHIP-20260322-0010",
      merchantCode: "MERCHANT-BETA",
      receiverPhone: "0933444555",
      receiverName: "Pham Gia An",
      senderName: "Pham Ngoc Hoa",
      status: "AwaitingPickup",
      serviceType: "Express",
      codAmount: 220000,
      shippingFee: 41000,
      totalFee: 41000,
      createdAt: "2026-03-21T09:40:00Z",
      updatedAt: "2026-03-21T10:10:00Z"
    },
    {
      shipmentId: "dc657a9f-3f41-4b43-b1ae-bc5ef7b3b655",
      trackingCode: "LGA2603000004",
      shipmentCode: "SHIP-20260320-0021",
      merchantCode: "MERCHANT-DELTA",
      receiverPhone: "0988777666",
      receiverName: "Bui Khanh Linh",
      senderName: "Hoang Gia Bao",
      status: "Cancelled",
      serviceType: "Standard",
      codAmount: 80000,
      shippingFee: 18000,
      totalFee: 18000,
      createdAt: "2026-03-20T07:30:00Z",
      updatedAt: "2026-03-20T07:50:00Z"
    }
  ]
};

export const mockTrackingSummary: TrackingSummary = {
  trackingCode: "LGA2603000001",
  shipmentCode: "SHIP-20260323-0001",
  currentStatus: "InTransit",
  receiverName: "Tran Thi B",
  lastUpdatedAt: "2026-03-23T14:00:00Z",
};

export const mockTrackingTimeline: TrackingTimelineResponse = {
  trackingCode: "LGA2603000001",
  events: [
    {
      eventId: "1",
      fromStatus: null,
      toStatus: "Created",
      hubCode: null,
      location: null,
      note: "Tao don",
      occurredAt: "2026-03-23T10:00:00Z",
    },
    {
      eventId: "2",
      fromStatus: "Created",
      toStatus: "AwaitingPickup",
      hubCode: "HUB-HCM-001",
      location: "Thu Duc, HCM",
      note: "Dang cho lay hang",
      occurredAt: "2026-03-23T10:30:00Z",
    },
    {
      eventId: "3",
      fromStatus: "AwaitingPickup",
      toStatus: "InTransit",
      hubCode: "SGN-HUB-01",
      location: "Ho Chi Minh City",
      note: "Da nhap kho trung chuyen",
      occurredAt: "2026-03-23T11:05:00Z",
    },
  ],
};