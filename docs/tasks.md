# Tasks Backlog — Logistics API (MVP)

> Quy ước:
> - **P0**: bắt buộc cho MVP production-ready
> - **P1**: nên có (MVP+)
> - **P2**: nice-to-have

> DoD chung (Definition of Done) cho mọi task:
> - [ ] `dotnet build Logistics.Api.sln` pass
> - [ ] `dotnet test Logistics.Api.sln` pass (nếu task có tests)
> - [ ] Có logging + correlation id phù hợp
> - [ ] Không nhét business logic vào controller
> - [ ] Có validation (nếu có input)
> - [ ] Có migration (nếu thay đổi DB)
> - [ ] Update docs (api/db) nếu thay đổi contract/schema

---

## A. Foundation / Platform (Cross-cutting)

### A1. Module registration pattern (P0)
- [x] Thiết kế interface `IModule` / `IModuleInstaller` để mỗi module tự register DI + endpoints
- [x] Host load modules theo assembly scanning hoặc explicit list
- Acceptance:
  - Host không chứa logic module-specific ngoài wiring
  - Module có entrypoint `Add<Module>()`
- ✅ Done: `IModule` marker interface in BuildingBlocks.Application; `AddIdentityModule()` extension in Identity.Infrastructure; wired in Program.cs

### A2. ProblemDetails mapping for Result/Validation (P0)
- [x] Implement middleware/filter map FluentValidation errors -> ProblemDetails (400)
- [x] Map `Result` failures -> ProblemDetails (400/409/404 tùy error code)
- Acceptance:
  - Response error consistent
  - Có `traceId` + `correlationId`
- ✅ Done: `ValidationPipelineBehavior` in BuildingBlocks.Application; `GlobalExceptionHandler` updated to catch `ValidationException` → `ValidationProblemDetails`; `ResultExtensions.ToProblemResult()` in Host for mapping Result errors.

### A3. Audit logging (P0)
- [ ] DB table `messaging.audit_logs` (EF mapping + migration)
- [ ] Middleware capture: userId, action, entity, old/new values (một phần)
- Acceptance:
  - Có record audit log khi CreateShipment, ChangeStatus, CancelShipment, CreateWebhookSubscription

### A4. Idempotency framework (P0)
- [ ] Implement `Idempotency-Key` handling cho create shipment (Redis + DB unique)
- [ ] Store request hash + response snapshot (tùy scope)
- Acceptance:
  - Gửi same key -> trả same response, không tạo record mới

### A5. Rate limiting policies (P0)
- [ ] Global fixed window (đã có)
- [ ] Add policy by endpoint group: `shipments-create`, `tracking-public`
- Acceptance:
  - 429 có ProblemDetails + retry-after header (nếu có)

### A6. Health checks hardening (P0)
- [ ] readiness/liveness endpoints chuẩn
- [ ] Add `/metrics` Prometheus
- Acceptance:
  - Docker local up -> health reports dependencies

### A7. OpenTelemetry spans enrichment (P1)
- [ ] Add custom spans tags: correlationId, merchantId
- [ ] Export to Jaeger (already)
- Acceptance:
  - Trace shows route + status code + correlationId

---

## B. Identity / Auth

### B1. Auth API: login/refresh/logout (P0)
- [x] `POST /api/v1/auth/login`
- [x] `POST /api/v1/auth/refresh`
- [x] `POST /api/v1/auth/logout`
- [x] Password verify BCrypt
- [x] Refresh token rotation + revoke old token
- Acceptance:
  - JWT contains `sub` userId + `role` claims + (optional) merchantId scope
  - Refresh token stored hashed, revoke works
- ✅ Done: Full Identity module (Domain/Application/Infrastructure/Migration/Controller).

### B2. RBAC authorization policies (P0)
- [x] Define roles: Admin, Operator, HubStaff, Merchant
- [x] Add `Authorize(Policy=...)` or role-based attributes per endpoint group
- Acceptance:
  - Merchant cannot access other merchant shipments
- ✅ Done: Policies registered in Program.cs (AdminOnly, OperatorOrAdmin, HubStaffOrAbove, MerchantOnly, MerchantOrAdmin). Role name constants in `Role.Names`.

### B3. Seed default roles + admin user (P1)
- [ ] Migration seed (or startup seeding with safe idempotent)
- Acceptance:
  - Local dev has default admin for testing

---

## C. Merchants

### C1. Merchant CRUD (Admin) (P1)
- [ ] Create merchant + generate merchantCode
- [ ] List/search merchants
- Acceptance:
  - Unique merchant code, store webhook secret

### C2. Merchant users link (P1)
- [ ] Create merchant-user relation (logical FK to identity.user)
- Acceptance:
  - A user can belong to multiple merchants (future)

---

## D. Pricing

### D1. PricingRule model + fee calculator (P0)
- [ ] DB table `pricing.pricing_rules` + migration
- [ ] Implement calculator:
  - zone type (same province / national)
  - weight bracket
  - COD fee percent
  - service type
- Acceptance:
  - CreateShipment returns computed fee fields

---

## E. Shipments (Core Domain)

### E1. Shipment aggregate + status machine (P0)
- [ ] Domain model: Shipment + TrackingEvent
- [ ] Define allowed transitions table (state machine)
- Acceptance:
  - Invalid transition -> 409 Conflict (ProblemDetails)

### E2. Create shipment command (P0)
- [ ] Endpoint `POST /api/v1/shipments`
- [ ] Idempotency-Key required
- [ ] Generate trackingCode + shipmentCode
- [ ] Persist shipment + tracking event "Created"
- Acceptance:
  - Duplicate request with same idempotency key returns same result

### E3. Cancel shipment (P0)
- [ ] Endpoint `POST /api/v1/shipments/{id}/cancel`
- [ ] Only allowed in certain states
- [ ] Append tracking event "Cancelled"
- Acceptance:
  - Cancelled shipments not movable

### E4. Status transitions (P0)
- [ ] Endpoint `POST /api/v1/shipments/{id}/status-transitions`
- [ ] Append tracking event
- [ ] Publish integration event (via outbox)
- Acceptance:
  - Each status change yields new tracking event row

### E5. Shipment query APIs (P0)
- [ ] `GET /api/v1/shipments/{id}`
- [ ] `GET /api/v1/shipments/by-tracking/{trackingCode}`
- [ ] `GET /api/v1/shipments` (filters, paging)
- Acceptance:
  - Merchant scope filter enforced

---

## F. Tracking

### F1. Public tracking lookup (P0)
- [ ] `GET /api/v1/tracking/{trackingCode}`
- [ ] `GET /api/v1/tracking/{trackingCode}/timeline`
- Acceptance:
  - No auth required, rate limit stricter

---

## G. Hubs / Warehouses

### G1. Hub CRUD (Admin/Operator) (P1)
- [ ] Create/update hub
- [ ] List hubs
- Acceptance:
  - Unique hub code

### G2. Assign shipment to hub (P1)
- [ ] API for hub staff to scan and assign
- [ ] Publish `ShipmentAssignedToHubIntegrationEvent`
- Acceptance:
  - Shipment current hub updated

---

## H. Messaging / Events

### H1. Integration event contracts (P0)
- [ ] Define events in `BuildingBlocks.Contracts`
- [ ] Envelope fields: eventId, correlationId, occurredOn, version
- Acceptance:
  - Stable schema + versioning

### H2. Outbox publisher worker (P0)
- [ ] Poll outbox table, publish RabbitMQ, mark processed
- [ ] Retry on transient failures + backoff
- Acceptance:
  - Publish is at-least-once

### H3. Inbox consumer idempotency (P0)
- [ ] Consumers record processed message ids
- Acceptance:
  - Duplicate broker deliveries do not duplicate side effects

### H4. DLQ strategy (P1)
- [ ] Configure RabbitMQ dead-letter exchange/queue
- [ ] Consumer failure -> retry then route to DLQ
- Acceptance:
  - Operator can inspect failed messages

---

## I. Webhooks / Notifications

### I1. Webhook subscription APIs (P0)
- [ ] CRUD subscription for merchant
- [ ] Store secret, callback url, events
- Acceptance:
  - Validate URL, event types

### I2. Webhook delivery worker + retry (P0)
- [ ] Persist deliveries
- [ ] Retry exponential backoff
- [ ] HMAC signature
- Acceptance:
  - Delivery logs include request/response; idempotent on eventId

### I3. Webhook test endpoint (P1)
- [ ] Send test payload to merchant
- Acceptance:
  - Useful for merchant integration

---

## J. Search (Elasticsearch)

### J1. Shipment search index mapping (P0)
- [ ] Define index settings/mappings
- [ ] Create index on startup (dev only) or via migration script
- Acceptance:
  - `trackingCode` keyword; names text; dates range

### J2. Indexing consumer (P0)
- [ ] Consume ShipmentCreated/StatusChanged, update ES doc
- Acceptance:
  - ES eventually consistent within seconds

### J3. Search API (P0)
- [ ] `GET /api/v1/search/shipments`
- [ ] filters + paging + sorting
- Acceptance:
  - Queries use ES, not PostgreSQL

---

## K. Reconciliation / COD

### K1. COD transaction record (P1)
- [ ] Create COD record when shipment has codAmount > 0
- [ ] Update COD status on Delivered/Returned
- Acceptance:
  - Basic COD lifecycle exists

### K2. Remittance batch (P2)
- [ ] Create batch for merchant payout
- Acceptance:
  - Exportable report

---

## L. Testing

### L1. Unit tests for Shipment state machine (P0)
- [ ] Valid transitions pass
- [ ] Invalid transitions fail
- Acceptance:
  - Coverage for core logic

### L2. Integration tests (P1)
- [ ] Testcontainers for postgres/redis/rabbitmq
- [ ] End-to-end create shipment + status change
- Acceptance:
  - `dotnet test` stable on CI

### L3. Architecture tests (P1)
- [ ] Enforce dependencies: Domain not reference Infrastructure
- Acceptance:
  - Prevents layer violations

---

## M. Documentation

### M1. Keep docs in sync (P0)
- [ ] Update `docs/api-contract.md` when endpoints change
- [ ] Update `docs/db-schema.md` when schema changes
- Acceptance:
  - Docs reflect real implementation