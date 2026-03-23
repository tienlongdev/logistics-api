# Technical Design — Logistics API (.NET 10)

## 1. Architecture Overview
### 1.1 Architectural style
- Clean Architecture + DDD thực dụng
- Modular Monolith (phase 1) — module boundaries rõ ràng
- Microservices-oriented design (phase 2 ready)

### 1.2 Modules / Bounded Contexts
- Identity/Auth
- Merchants/Customers
- Shipments (core domain)
- Tracking
- Hubs/Warehouses
- Pricing
- Notifications/Webhooks
- Search
- Reconciliation/COD

### 1.3 Communication patterns
- In-process: Application layer → Domain (trong cùng module)
- Cross-module: Integration Events (RabbitMQ) + Outbox Pattern
- Không truy cập trực tiếp Domain model module khác (tránh coupling)

---

## 2. Key technical decisions

### 2.1 EF Core + PostgreSQL
- PostgreSQL là source of truth
- Migrations-only (không EnsureCreated)
- Schema-per-module để dễ extract service

### 2.2 CQRS (pragmatic)
- Commands/Queries qua MediatR (Application layer)
- Không bắt buộc tách DB read/write
- Query performance-critical có thể dùng Dapper (sau)

### 2.3 Domain Events vs Integration Events
- Domain events: nội bộ module, side-effects trong cùng process
- Integration events: contract ổn định, đi qua outbox + broker để module khác consume

### 2.4 Outbox/Inbox
- Outbox: đảm bảo publish event “at least once”
- Inbox: consumer idempotency, tránh duplicate side-effects

### 2.5 Webhook reliability
- Persist `webhook_deliveries`
- Retry exponential backoff
- Dead-letter (Exhausted) để operator xử lý
- HMAC signature để merchant verify

### 2.6 Observability
- Correlation ID: `X-Correlation-Id`
- Serilog structured logs + Seq
- OpenTelemetry traces (OTLP) + Jaeger
- Prometheus metrics + Grafana dashboards
- Health checks readiness/liveness

---

## 3. Data flows (high level)

### 3.1 Create Shipment
1) API receives request (Idempotency-Key)
2) Validate input (FluentValidation)
3) Calculate fees (Pricing module)
4) Persist Shipment + TrackingEvent(Created) in PostgreSQL
5) Create OutboxMessage(ShipmentCreatedIntegrationEvent)
6) Commit transaction
7) Outbox worker publishes event to RabbitMQ
8) Search consumer updates Elasticsearch document
9) Webhook worker schedules deliveries (if subscribed)

### 3.2 Shipment Status Transition
1) Operator/HubStaff calls transition endpoint
2) Validate transition (state machine)
3) Append tracking event, update current status/hub
4) Persist + outbox event (ShipmentStatusChangedIntegrationEvent)
5) Workers react: index update, webhook deliveries, audit logging

---

## 4. Security
- JWT access token + refresh token (hashed)
- RBAC claims: `role`
- Merchant scope: `merchantId` claim (recommended) hoặc lookup mapping
- Rate limiting: global + per endpoint policy

---

## 5. Local deployment
- Docker compose stack: postgres, redis, rabbitmq, elasticsearch, kibana, seq, jaeger, prometheus, grafana
- App runs: Host API + background workers (future separate processes)

---

## 6. Microservices evolution plan
- Mỗi module có DbContext riêng + schema riêng
- Integration events đã define boundaries
- Khi extract:
  - module → service
  - schema → own DB
  - replace in-process call bằng event/HTTP/gRPC (nếu cần)
  - keep event contracts stable (versioned)