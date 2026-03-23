# API Contract — Logistics API (v1)

> Base path: `/api/v{version}` (ví dụ: `/api/v1`)

## 0. Conventions
### Headers
- `Authorization: Bearer <jwt>`
- `X-Correlation-Id: <id>` (optional; server generates if missing)
- `Idempotency-Key: <key>` (required for create shipment)

### Response conventions
- Success: JSON DTO
- Errors: `application/problem+json` (`ProblemDetails`) với `traceId` trong extensions

### Pagination (gợi ý)
- `page`: 1-based
- `pageSize`: max 100
- Response include: `items`, `page`, `pageSize`, `total`

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

### POST `/api/v1/auth/logout`
Request
```json
{ "refreshToken": "<opaque>" }
```

## 2. Merchants
### GET `/api/v1/merchants/me` (Merchant)
Response
```json
{
  "merchantId": "uuid",
  "merchantCode": "MCH0001",
  "name": "Demo Merchant",
  "email": "merchant@demo.com",
  "phone": "090..."
}
```

## 3. Shipments
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
{
  "reason": "Merchant requested cancellation"
}
```

### POST `/api/v1/shipments/{id}/status-transitions`
Request
```json
{
  "toStatus": "PickedUp",
  "hubCode": "HUB-HCM-001",
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
  "occurredAt": "2026-03-23T11:00:00Z"
}
```

## 4. Tracking (public)
### GET `/api/v1/tracking/{trackingCode}`
Response
```json
{
  "trackingCode": "LGA2603000001",
  "currentStatus": "InTransit",
  "lastUpdatedAt": "2026-03-23T14:00:00Z"
}
```

### GET `/api/v1/tracking/{trackingCode}/timeline`
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
      "note": "Tạo đơn",
      "occurredAt": "2026-03-23T10:00:00Z"
    }
  ]
}
```

## 5. Hubs
### POST `/api/v1/hubs` (Admin)
Request
```json
{
  "code": "HUB-HCM-001",
  "name": "Kho HCM Quận 1",
  "hubType": "OriginHub",
  "province": "HCM",
  "district": "Q1",
  "address": "12 ..."
}
```

## 6. Search
### GET `/api/v1/search/shipments`
Query params:
- `q` (full text)
- `trackingCode`, `shipmentCode`
- `merchantCode`
- `receiverPhone`
- `status`
- `fromDate`, `toDate`
- `page`, `pageSize`
- `sort` (e.g. `createdAt:desc`)

Response:
```json
{
  "items": [],
  "page": 1,
  "pageSize": 20,
  "total": 0
}
```

## 7. Webhooks
### POST `/api/v1/webhooks/subscriptions`
Request
```json
{
  "callbackUrl": "https://merchant.com/webhook",
  "events": ["ShipmentStatusChanged", "ShipmentCreated"]
}
```

### Signature convention (HMAC)
- Header: `X-Webhook-Signature: sha256=<hex>`
- Signature = HMACSHA256(secret, raw_body)

## 8. Reconciliation
### GET `/api/v1/reconciliation/cod-transactions`
Filters: `merchantCode`, `status`, `fromDate`, `toDate`