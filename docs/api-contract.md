# API Contract — Logistics API (v1)

> Base path: `/api/v{version}` (ví dụ: `/api/v1`)

## 0. Conventions
### 0.1 Headers
- `Authorization: Bearer <jwt>`
- `X-Correlation-Id: <id>` (optional; server generates if missing)
- `Idempotency-Key: <key>` (required for create shipment)

### 0.2 Response conventions
- Success: JSON DTO
- Errors: `application/problem+json` (ProblemDetails) có `traceId` + `correlationId`

### 0.3 Pagination (recommended)
Request query:
- `page` (1-based)
- `pageSize` (max 100)

Response:
```json
{
  "items": [],
  "page": 1,
  "pageSize": 20,
  "total": 0
}
```

---

## 1. Auth

### POST `/api/v1/auth/login`
Request
```json
{
  "email": "merchant@demo.com",
  "password": "P@ssw0rd!"
}
```

Response
```json
{
  "accessToken": "<jwt>",
  "refreshToken": "<opaque>",
  "expiresIn": 3600
}
```

### POST `/api/v1/auth/refresh`
Request
```json
{ "refreshToken": "<opaque>" }
```

Response
```json
{
  "accessToken": "<jwt>",
  "refreshToken": "<opaque-new>",
  "expiresIn": 3600
}
```

### POST `/api/v1/auth/logout`
Request
```json
{ "refreshToken": "<opaque>" }
```

Response: `204 No Content`

---

## 2. Shipments

### POST `/api/v1/shipments`
Headers:
- `Idempotency-Key: <string>`

Request
```json
{
  "merchantOrderRef": "ORDER-123",
  "serviceType": "Standard",
  "sender": {
    "name": "Nguyễn Văn A",
    "phone": "0901234567",
    "address": "12 ...",
    "province": "HCM",
    "district": "Q1",
    "ward": "Bến Nghé"
  },
  "receiver": {
    "name": "Trần Thị B",
    "phone": "0912345678",
    "address": "34 ...",
    "province": "Hà Nội",
    "district": "Cầu Giấy",
    "ward": "Dịch Vọng"
  },
  "package": {
    "weightGram": 1200,
    "lengthCm": 20,
    "widthCm": 10,
    "heightCm": 8,
    "description": "Books"
  },
  "codAmount": 500000,
  "declaredValue": 700000,
  "deliveryNote": "Giao giờ hành chính"
}
```

Response
```json
{
  "shipmentId": "uuid",
  "trackingCode": "LGA2603000001",
  "shipmentCode": "SHIP-20260323-0001",
  "currentStatus": "Created",
  "shippingFee": 35000,
  "codFee": 0,
  "totalFee": 35000,
  "createdAt": "2026-03-23T10:00:00Z"
}
```

### GET `/api/v1/shipments/{id}`
Response: shipment detail

### GET `/api/v1/shipments/by-tracking/{trackingCode}`
Response: shipment detail

### POST `/api/v1/shipments/{id}/cancel`
Request
```json
{ "reason": "Merchant requested cancellation" }
```

Response: shipment summary

### POST `/api/v1/shipments/{id}/status-transitions`
Auth:
- `Authorization: Bearer <jwt>`
- Role: `HubStaff`, `Operator`, or `Admin`

Request
```json
{
  "toStatus": "PickedUp",
  "hubId": "uuid-or-null",
  "hubCode": "HUB-HCM-001",
  "location": "Thu Duc, HCM",
  "note": "Đã lấy hàng",
  "occurredAt": "2026-03-23T11:00:00Z"
}
```

Response
```json
{
  "shipmentId": "uuid",
  "trackingCode": "LGA2603000001",
  "fromStatus": "AwaitingPickup",
  "toStatus": "PickedUp",
  "occurredAt": "2026-03-23T11:00:00Z",
  "currentStatus": "PickedUp"
}
```

---

## 3. Tracking (public)

### GET `/api/v1/tracking/{trackingCode}`
Auth: none

Response
```json
{
  "trackingCode": "LGA2603000001",
  "shipmentCode": "SHIP-20260323-0001",
  "currentStatus": "InTransit",
  "receiverName": "Trần Thị B",
  "lastUpdatedAt": "2026-03-23T14:00:00Z"
}
```

### GET `/api/v1/tracking/{trackingCode}/timeline`
Auth: none

Response
```json
{
  "trackingCode": "LGA2603000001",
  "events": [
    {
      "eventId": "uuid",
      "fromStatus": null,
      "toStatus": "Created",
      "hubCode": null,
      "location": null,
      "note": "Tạo đơn",
      "occurredAt": "2026-03-23T10:00:00Z"
    }
  ]
}
```

Notes:
- Timeline is append-only and immutable; events are returned oldest → newest.

---

## 4. Webhooks

Auth: merchant JWT required for all subscription management endpoints.

Supported events:
- `ShipmentCreated`
- `ShipmentStatusChanged`

### GET `/api/v1/webhooks/subscriptions`
Response
```json
[
  {
    "id": "2d3f0ffd-1084-409a-a65e-45499ef7d5c0",
    "callbackUrl": "https://merchant.example.com/webhooks/logistics",
    "events": ["ShipmentStatusChanged", "ShipmentCreated"],
    "isActive": true,
    "createdAt": "2026-03-23T10:00:00Z",
    "updatedAt": "2026-03-23T10:00:00Z"
  }
]
```

### GET `/api/v1/webhooks/subscriptions/{id}`
Response
```json
{
  "id": "2d3f0ffd-1084-409a-a65e-45499ef7d5c0",
  "callbackUrl": "https://merchant.example.com/webhooks/logistics",
  "events": ["ShipmentStatusChanged", "ShipmentCreated"],
  "isActive": true,
  "createdAt": "2026-03-23T10:00:00Z",
  "updatedAt": "2026-03-23T10:00:00Z"
}
```

### POST `/api/v1/webhooks/subscriptions`
Request
```json
{
  "callbackUrl": "https://merchant.example.com/webhooks/logistics",
  "events": ["ShipmentStatusChanged", "ShipmentCreated"]
}
```

Response `201 Created`
```json
{
  "id": "2d3f0ffd-1084-409a-a65e-45499ef7d5c0",
  "callbackUrl": "https://merchant.example.com/webhooks/logistics",
  "events": ["ShipmentStatusChanged", "ShipmentCreated"],
  "isActive": true,
  "signingSecret": "9c98a0b0cf0b97f5905f2c85b665a0fd9c28c3ea908f7c12a5f53b85043d4f83",
  "createdAt": "2026-03-23T10:00:00Z",
  "updatedAt": "2026-03-23T10:00:00Z"
}
```

Notes:
- `callbackUrl` must be HTTPS in normal environments.
- HTTP is accepted only for local development targets such as `http://localhost:3000/webhooks`.
- `signingSecret` is returned only once on creation.

### PUT `/api/v1/webhooks/subscriptions/{id}`
Request
```json
{
  "callbackUrl": "https://merchant.example.com/webhooks/logistics-v2",
  "events": ["ShipmentStatusChanged"],
  "isActive": true
}
```

Response
```json
{
  "id": "2d3f0ffd-1084-409a-a65e-45499ef7d5c0",
  "callbackUrl": "https://merchant.example.com/webhooks/logistics-v2",
  "events": ["ShipmentStatusChanged"],
  "isActive": true,
  "createdAt": "2026-03-23T10:00:00Z",
  "updatedAt": "2026-03-23T10:15:00Z"
}
```

### DELETE `/api/v1/webhooks/subscriptions/{id}`
Response: `204 No Content`

### Delivery payloads
Webhook body is the raw integration event envelope published by the platform.

Example `ShipmentStatusChanged` delivery:
```json
{
  "eventId": "f8e8f0ca-d86d-46d3-ae34-ae10f233e84d",
  "correlationId": "d6b5b2f7-e8ea-4c36-8bd1-c0fa2f073421",
  "occurredOn": "2026-03-23T11:05:00Z",
  "version": 1,
  "payload": {
    "shipmentId": "1c84ac03-0aef-46c6-92bf-f52a3cd6b568",
    "trackingCode": "LGA2603000001",
    "shipmentCode": "SHIP-20260323-0001",
    "fromStatus": "Created",
    "toStatus": "InTransit",
    "hubId": "5ad0ed67-f7c9-43c7-8a62-fd3ac4046db2",
    "hubCode": "SGN-HUB-01",
    "location": "Ho Chi Minh City",
    "note": "Đã nhập kho trung chuyển",
    "operatorId": "0df8b5aa-a2b3-4f2b-a9af-6fe90f9f34ee",
    "operatorName": "Hub Staff A",
    "occurredAt": "2026-03-23T11:05:00Z"
  }
}
```

### Signature convention (HMAC)
- Header: `X-Webhook-Signature: sha256=<hex>`
- Signature = `HMACSHA256(secret, raw_body)`
- Additional headers:
  - `X-Webhook-Event`
  - `X-Webhook-Event-Id`

### Delivery reliability
- Webhook deliveries are persisted in `notifications.webhook_deliveries` before sending.
- Failed deliveries are retried with exponential backoff.
- When retries are exhausted, delivery status becomes `Exhausted` for operator review.
- Request/response metadata is logged, but signing secrets are never written to logs.

---

## 5. Search

### GET `/api/v1/search/shipments`
Query params:
- `q`
- `trackingCode`
- `shipmentCode`
- `merchantCode`
- `receiverPhone`
- `status`
- `fromDate`, `toDate`
- `page`, `pageSize`
- `sort` (e.g. `createdAt:desc`)