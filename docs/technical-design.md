# Technical Design — Logistics API (.NET 10)

## 1. Architecture Overview
### 1.1 Style
- Clean Architecture + DDD thực dụng
- Modular Monolith (phase 1) — module boundaries rõ ràng
- Microservices-oriented design (phase 2 ready)

### 1.2 Modules / Bounded Contexts
- Identity/Auth
- Merchants/Customers
- Shipments (core)
- Tracking
- Hubs/Warehouses
- Pricing
- Notifications/Webhooks
- Search
- Reconciliation/COD

### 1.3 Communication patterns
- In-process: Application layer gọi domain (same module)
- Cross-module: ưu tiên Integration Events (RabbitMQ) + Outbox Pattern
- Không truy cập trực tiếp Domain model module khác

## 2. Key technical decisions
### 2.1 EF Core + PostgreSQL
- DB source of truth
- Migrations-only (no EnsureCreated)
- Schema per module (identity, shipments, ...)

### 2.2 CQRS (pragmatic)
- Commands/Queries qua MediatR (Application layer)
- Không bắt buộc tách DB read/write; chỉ tách handler + DTOs
- Một số query hiệu năng cao có thể dùng Dapper

### 2.3 Domain Events vs Integration Events
- Domain events: nội bộ module, dùng cho side-effects trong cùng process
- Integration events: contract ổn định, đi qua outbox + broker để module khác consume

### 2.4 Outbox/Inbox
- OutboxMessage: đảm bảo publish event “at least once”
- InboxMessage: consumer idempotency, store processed message ids

### 2.5 Webhook reliability
- WebhookDelivery persisted
- Retry background worker + exponential backoff
- Dead-letter (Exhausted) để operator xử lý
- HMAC signature để merchant verify

### 2.6 Observability
- Correlation ID (X-Correlation-Id)
- Serilog structured logs + Seq
- OpenTelemetry traces (OTLP) + Jaeger
- Prometheus metrics + Grafana dashboards
- Health checks readiness/liveness

## 3. Data flow (high level)
### 3.1 Create Shipment
1) API receives request (Idempotency-Key)
2) Validate input (FluentValidation)
3) Calculate fees (Pricing module)
4) Persist Shipment + TrackingEvent(Created) in PostgreSQL
5) Create OutboxMessage(ShipmentCreatedIntegrationEvent)
6) Commit transaction
7) Outbox worker publishes event to RabbitMQ
8) Search worker updates Elasticsearch index
9) Webhook worker schedules deliveries (if subscribed)

### 3.2 Status Transition
1) Operator/HubStaff calls status transition endpoint
2) Validate transition (state machine)
3) Append tracking event, update current status/hub
4) Persist + outbox (ShipmentStatusChangedIntegrationEvent)
5) Workers react: index update, webhook deliveries, audit logging

## 4. Security
- JWT access token + refresh token (hashed in DB)
- RBAC claims: role + merchant scope
- Rate limiting global + per merchant (roadmap)
- Idempotency key for shipment creation
- Sensitive data redaction in logs (roadmap)

## 5. Deployment (local)
- Docker Compose: postgres, redis, rabbitmq, elasticsearch, kibana, seq, jaeger, prometheus, grafana
- App runs as Host API + background workers (can be separate processes later)

## 6. Microservices evolution plan
- Keep each module self-contained with its own DbContext and schema
- Integration events already define boundaries
- When extracting:
  - split to separate repo/service
  - move schema to its own DB
  - replace in-process calls with HTTP/gRPC only where needed
  - keep event contracts stable