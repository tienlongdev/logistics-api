# Product Requirements — Logistics API (Enterprise)

## 1. Mục tiêu sản phẩm
Xây dựng backend cho một nền tảng logistics/shipping có thể mở rộng, phục vụ mô hình:
- Merchant/Customer tạo đơn giao hàng (shipment)
- Hệ thống vận hành hub/kho và tuyến vận chuyển
- Tracking gần-real-time theo tracking code
- Hỗ trợ COD, phí ship, đối soát cơ bản
- Webhook/callback cho merchant khi trạng thái đổi
- Search nhanh qua Elasticsearch

MVP định hướng “production-ready”: maintainable, testable, dễ chạy local, sẵn sàng tách microservices về sau.

## 2. Personas & Roles
### 2.1 Roles (RBAC)
- **Admin**: quản trị hệ thống, cấu hình global, quản lý merchant/hub/pricing, xem mọi dữ liệu.
- **Operator**: vận hành tổng, can thiệp shipment, chạy đối soát, xử lý ngoại lệ.
- **HubStaff**: nhân viên hub/kho, scan/gán hub, cập nhật trạng thái tại hub.
- **Merchant**: tạo shipment, xem shipment của merchant, cấu hình webhook, xem đối soát.

### 2.2 Tenancy
- MVP: single-tenant (1 instance dùng cho nhiều merchants)
- Roadmap: multi-tenant (database schema hoặc DB-per-tenant), nhưng thiết kế từ đầu cần:
  - MerchantId là first-class field
  - Security filters theo MerchantId cho role Merchant
  - Event payload luôn chứa MerchantId/MerchantCode

## 3. Phạm vi nghiệp vụ (MVP)
### 3.1 Shipment / Order
Merchant có thể tạo shipment với:
- Sender info: name, phone, address, province/district/ward
- Receiver info: name, phone, address, province/district/ward
- Pickup address, delivery address (MVP có thể unify với sender/receiver address)
- Package: weight, dimensions, description
- COD amount, declared value
- Delivery instructions (note)
- Service type: Standard / Express

Yêu cầu:
- Sinh **trackingCode** duy nhất
- Sinh **shipmentCode / order code** theo format
- Tính phí ship cơ bản theo weight/zone/COD/service type
- Cho phép hủy đơn trong trạng thái hợp lệ (rule theo state machine)

### 3.2 Tracking
Mỗi shipment có tracking timeline (tracking events). Các trạng thái mẫu:

- Created
- AwaitingPickup
- PickedUp
- InboundAtOriginHub
- InTransit
- InboundAtDestinationHub
- OutForDelivery
- DeliveryAttemptFailed
- Delivered
- Cancelled
- Returned
- ReturnInTransit
- ReturnedToSender

Yêu cầu khi đổi status:
- Validate state transition (state machine)
- Append tracking event (immutable history)
- Publish integration event
- Update Elasticsearch index
- Ghi audit log

API:
- Lookup theo trackingCode (public)
- Lấy timeline đầy đủ

### 3.3 Hub / Warehouse
- CRUD hub/kho (admin)
- Gán shipment vào hub (hub staff/operator)
- Routing đơn giản ban đầu (rule-based, có thể nâng cấp rule engine)

### 3.4 Webhook
- Merchant đăng ký callback URL + subscribe event types
- Khi shipment đổi trạng thái → trigger webhook
- Reliability:
  - Persist webhook delivery record
  - Retry + exponential backoff
  - Dead-letter khi exhausted
  - Log request/response (redact PII nếu cần)
- Security:
  - HMAC signature (merchant verify)

### 3.5 Search
Search shipments theo:
- trackingCode
- shipmentCode
- receiverPhone
- merchantCode
- status
- date range
- (optional) receiverName/senderName full text

Elasticsearch phục vụ search. PostgreSQL là source of truth.

### 3.6 COD & Reconciliation
- Ghi nhận COD transaction per shipment
- Tổng hợp theo merchant + date range
- Tạo remittance batch (MVP basic)
- Export/report (roadmap)

## 4. Non-functional requirements (NFR)
- **Reliability**: outbox pattern cho events, webhook retry; idempotency cho create shipment
- **Observability**: structured logging, correlation id, tracing, metrics, health checks
- **Security**: JWT + refresh token; RBAC; rate limiting; input validation
- **Performance**:
  - Search qua Elasticsearch
  - Read models có thể optimize (CQRS hợp lý)
- **Data correctness**:
  - PostgreSQL authoritative
  - Tracking events immutable

## 5. Out of scope (MVP)
- Real-time push (websocket) cho tracking (có thể roadmap)
- Payment gateway
- Advanced route optimization, dynamic pricing engine
- Full-blown event sourcing

## 6. Milestones đề xuất
1) Foundation + observability + health checks  
2) Identity/Auth + RBAC  
3) Shipments core + status transitions + tracking  
4) Outbox + RabbitMQ + worker  
5) Webhooks + retry  
6) Elasticsearch indexing + search API  
7) COD reconciliation basic  
8) Testing + docs + hardening