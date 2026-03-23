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
- Signature = `HMACSHA256(secret, raw_body)`

---

## 5. Search

### GET `/api/v1/search/shipments`
Auth: JWT required

Behavior:
- Search is served from Elasticsearch, not PostgreSQL.
- PostgreSQL remains the source of truth.
- Shipment create/status events trigger reindexing into Elasticsearch.
- For merchant users, `merchantCode` is forced to the caller's merchant scope.

Query params:
- `trackingCode`
- `shipmentCode`
- `merchantCode`
- `receiverPhone`
- `status`
- `fromDate`, `toDate`
- `page`, `pageSize`
- `sort` (e.g. `createdAt:desc`)

Supported sort fields:
- `createdAt`
- `updatedAt`
- `trackingCode`
- `shipmentCode`
- `merchantCode`
- `receiverPhone`
- `status`

Response
```json
{
  "total": 2,
  "page": 1,
  "pageSize": 20,
  "sort": "createdAt:desc",
  "items": [
    {
      "shipmentId": "0f8fad5b-d9cb-469f-a165-70867728950e",
      "trackingCode": "LGA2603000001",
      "shipmentCode": "SHIP-20260323-0001",
      "merchantCode": "MERCHANT-ACME",
      "receiverPhone": "0909123456",
      "receiverName": "Tran Thi B",
      "senderName": "Nguyen Van A",
      "status": "InTransit",
      "serviceType": "Express",
      "codAmount": 150000,
      "shippingFee": 32000,
      "totalFee": 32000,
      "createdAt": "2026-03-23T10:00:00Z",
      "updatedAt": "2026-03-23T11:05:00Z"
    }
  ]
}
```

Example
```http
GET /api/v1/search/shipments?merchantCode=MERCHANT-ACME&status=InTransit&fromDate=2026-03-01T00:00:00Z&toDate=2026-03-31T23:59:59Z&page=1&pageSize=20&sort=createdAt:desc
Authorization: Bearer <access-token>
```

### Local Elasticsearch index strategy
- Worker startup ensures the shipments index exists.
- Worker startup can backfill the shipments index from PostgreSQL for local development.
- Default local index name: `logistics-shipments-v1`.
- To rebuild locally, delete the index and restart the worker.

Example local rebuild command:
```bash
curl -X DELETE http://localhost:9200/logistics-shipments-v1
```