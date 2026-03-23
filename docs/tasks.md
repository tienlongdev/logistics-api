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

=== AUDIT SUMMARY ===
✅ Done:
- A1. Module registration pattern — evidence: `src/BuildingBlocks/Logistics.Api.BuildingBlocks.Application/Abstractions/IModule.cs`; `src/Services/Identity/Logistics.Api.Identity.Infrastructure/IdentityInfrastructureModule.cs`
- A2. ProblemDetails mapping for Result/Validation — evidence: `src/BuildingBlocks/Logistics.Api.BuildingBlocks.Application/Behaviors/ValidationPipelineBehavior.cs`; `src/Host/Logistics.Api.Host/Middleware/GlobalExceptionHandler.cs`
- B1. Auth API: login/refresh/logout — evidence: `src/Host/Logistics.Api.Host/Controllers/Auth/AuthController.cs`; `src/Services/Identity/Logistics.Api.Identity.Application/Commands/Login/LoginCommandHandler.cs`
- B2. RBAC authorization policies — evidence: `src/Host/Logistics.Api.Host/Program.cs`; `src/Services/Identity/Logistics.Api.Identity.Domain/Entities/Role.cs`
- D1. PricingRule model + fee calculator — evidence: `src/Services/Pricing/Logistics.Api.Pricing.Infrastructure/Migrations/20260323030750_InitialPricingSchema.cs`; `src/Services/Pricing/Logistics.Api.Pricing.Application/Services/PricingCalculator.cs`
- E1. Shipment aggregate + status machine — evidence: `src/Services/Shipments/Logistics.Api.Shipments.Domain/Entities/Shipment.cs`; `src/Services/Shipments/Logistics.Api.Shipments.Domain/Services/ShipmentStateMachine.cs`
- E2. Create shipment command — evidence: `src/Host/Logistics.Api.Host/Controllers/Shipments/ShipmentsController.cs`; `src/Services/Shipments/Logistics.Api.Shipments.Application/Commands/CreateShipment/CreateShipmentCommandHandler.cs`
- E4. Status transitions — evidence: `src/Host/Logistics.Api.Host/Controllers/Shipments/ShipmentStatusTransitionsController.cs`; `src/Services/Shipments/Logistics.Api.Shipments.Application/Commands/TransitionShipmentStatus/TransitionShipmentStatusCommandHandler.cs`
- F1. Public tracking lookup — evidence: `src/Host/Logistics.Api.Host/Controllers/Tracking/TrackingController.cs`; `src/Services/Shipments/Logistics.Api.Shipments.Application/Queries/GetTrackingSummary/GetTrackingSummaryQueryHandler.cs`
- H1. Integration event contracts — evidence: `src/BuildingBlocks/Logistics.Api.BuildingBlocks.Contracts/IntegrationEventEnvelope.cs`; `src/BuildingBlocks/Logistics.Api.BuildingBlocks.Contracts/ShipmentIntegrationEvents.cs`
- H2. Outbox publisher worker — evidence: `src/Workers/Logistics.Api.Messaging.Worker/Program.cs`; `src/Workers/Logistics.Api.Messaging.Worker/Services/OutboxPublisherBackgroundService.cs`
- I1. Webhook subscription APIs — evidence: `src/Host/Logistics.Api.Host/Controllers/Webhooks/WebhookSubscriptionsController.cs`; `src/Services/Notifications/Logistics.Api.Notifications.Application/Commands/CreateWebhookSubscription/CreateWebhookSubscriptionCommandHandler.cs`
- J1. Shipment search index mapping — evidence: `src/Services/Search/Logistics.Api.Search.Infrastructure/Services/ElasticsearchShipmentSearchService.cs`; `src/Services/Search/Logistics.Api.Search.Infrastructure/Services/ShipmentSearchIndexInitializer.cs`
- J2. Indexing consumer — evidence: `src/Services/Search/Logistics.Api.Search.Infrastructure/Messaging/ShipmentCreatedSearchIndexConsumer.cs`; `src/Workers/Logistics.Api.Messaging.Worker/Program.cs`
- J3. Search API — evidence: `src/Host/Logistics.Api.Host/Controllers/Search/SearchController.cs`; `src/Services/Search/Logistics.Api.Search.Application/Queries/SearchShipments/SearchShipmentsQueryHandler.cs`
- L1. Unit tests for Shipment state machine — evidence: `tests/Logistics.Api.UnitTests/Shipments/ShipmentStateMachineTests.cs`

🟡 Partial:
- A4. Idempotency framework
  Partial: CreateShipment requires `Idempotency-Key`, checks Redis + DB, and uses a unique DB constraint to return the same shipment response.
  Missing: request hash persistence and response snapshot storage.
  Evidence: `src/Host/Logistics.Api.Host/Controllers/Shipments/ShipmentsController.cs`; `src/Services/Shipments/Logistics.Api.Shipments.Infrastructure/Idempotency/RedisIdempotencyService.cs`; `src/Services/Shipments/Logistics.Api.Shipments.Infrastructure/Persistence/Configurations/ShipmentConfiguration.cs`
- A5. Rate limiting policies
  Partial: global fixed-window rate limiting is registered.
  Missing: endpoint-group policies for `shipments-create` and `tracking-public`, plus explicit retry-after handling.
  Evidence: `src/Host/Logistics.Api.Host/Program.cs`; `src/Host/Logistics.Api.Host/Extensions/RateLimitingExtensions.cs`
- A6. Health checks hardening
  Partial: `/health`, `/health/live`, and `/health/ready` are mapped with dependency checks.
  Missing: `/metrics` endpoint.
  Evidence: `src/Host/Logistics.Api.Host/Extensions/HealthChecksExtensions.cs`; `src/Host/Logistics.Api.Host/Program.cs`
- A7. OpenTelemetry spans enrichment
  Partial: OTLP export is configured for Jaeger/OpenTelemetry Collector.
  Missing: custom span tags for `correlationId` and `merchantId`.
  Evidence: `src/BuildingBlocks/Logistics.Api.BuildingBlocks.Observability/Observability/OpenTelemetry/OpenTelemetryBootstrapper.cs`; `src/Host/Logistics.Api.Host/Program.cs`
- C2. Merchant users link
  Partial: merchant-user relation table, EF mapping, and migration exist.
  Missing: application/API flow to create or manage merchant-user links.
  Evidence: `src/Services/Merchants/Logistics.Api.Merchants.Infrastructure/Persistence/Configurations/MerchantUserEntityConfiguration.cs`; `src/Services/Merchants/Logistics.Api.Merchants.Infrastructure/Migrations/20260323033509_InitialMerchantsSchema.cs`
- E3. Cancel shipment
  Partial: the shipment state machine supports `Cancelled` and appends tracking events through the generic transition flow.
  Missing: dedicated `POST /api/v1/shipments/{id}/cancel` endpoint and cancel-specific command/validation.
  Evidence: `src/Services/Shipments/Logistics.Api.Shipments.Domain/Services/ShipmentStateMachine.cs`; `src/Services/Shipments/Logistics.Api.Shipments.Domain/Entities/Shipment.cs`; `src/Host/Logistics.Api.Host/Controllers/Shipments/ShipmentStatusTransitionsController.cs`
- E5. Shipment query APIs
  Partial: repository/query primitives exist for lookup by shipment id and tracking code.
  Missing: `GET /api/v1/shipments/{id}`, `GET /api/v1/shipments/by-tracking/{trackingCode}`, and `GET /api/v1/shipments` endpoints with merchant-scoped filtering/paging.
  Evidence: `src/Services/Shipments/Logistics.Api.Shipments.Domain/Repositories/IShipmentRepository.cs`; `src/Services/Shipments/Logistics.Api.Shipments.Infrastructure/Repositories/ShipmentRepository.cs`
- H3. Inbox consumer idempotency
  Partial: inbox table, EF mapping, migration, and MassTransit filter exist.
  Missing: concrete DI/consumer pipeline registration applying the inbox filter.
  Evidence: `src/Services/Notifications/Logistics.Api.Notifications.Infrastructure/Messaging/InboxIdempotencyFilter.cs`; `src/Services/Notifications/Logistics.Api.Notifications.Infrastructure/Migrations/20260323045133_InitialNotificationsWebhooks.cs`
- I2. Webhook delivery worker + retry
  Partial: deliveries are persisted, retried with exponential backoff, and signed with HMAC.
  Missing: DI/MassTransit/hosted-service registration wiring the notification consumers and delivery worker into a running process.
  Evidence: `src/Services/Notifications/Logistics.Api.Notifications.Infrastructure/Messaging/ShipmentCreatedIntegrationEventConsumer.cs`; `src/Services/Notifications/Logistics.Api.Notifications.Infrastructure/Services/WebhookDeliveryBackgroundService.cs`
- M1. Keep docs in sync
  Partial: `docs/api-contract.md` covers the implemented auth, create-shipment, status-transition, tracking, webhook subscription, and search endpoints.
  Missing: shipment detail/by-tracking/cancel endpoints are still documented but not implemented; `/health`, `/health/live`, `/health/ready`, `/openapi/{documentName}.json`, and `/swagger` are undocumented; `docs/db-schema.md` was not updated as part of the audited implementation.
  Evidence: `docs/api-contract.md`; `src/Host/Logistics.Api.Host/Controllers`; `src/Host/Logistics.Api.Host/Extensions`

❌ Not started:
- A3. Audit logging
- B3. Seed default roles + admin user
- C1. Merchant CRUD (Admin)
- G1. Hub CRUD (Admin/Operator)
- G2. Assign shipment to hub
- H4. DLQ strategy
- I3. Webhook test endpoint
- K1. COD transaction record
- K2. Remittance batch
- L2. Integration tests
- L3. Architecture tests

=== ENDPOINTS DISCOVERED ===
- POST /api/v1/auth/login
- POST /api/v1/auth/refresh
- POST /api/v1/auth/logout
- POST /api/v1/shipments
- POST /api/v1/shipments/{id}/status-transitions
- GET /api/v1/tracking/{trackingCode}
- GET /api/v1/tracking/{trackingCode}/timeline
- GET /api/v1/search/shipments
- GET /api/v1/webhooks/subscriptions
- GET /api/v1/webhooks/subscriptions/{id}
- POST /api/v1/webhooks/subscriptions
- PUT /api/v1/webhooks/subscriptions/{id}
- DELETE /api/v1/webhooks/subscriptions/{id}
- GET /health
- GET /health/live
- GET /health/ready
- GET /openapi/{documentName}.json
- GET /swagger

=== CONTRACT GAP ===
Missing in implementation:
- GET /api/v1/shipments/{id}
- GET /api/v1/shipments/by-tracking/{trackingCode}
- POST /api/v1/shipments/{id}/cancel

Extra (not documented):
- GET /health
- GET /health/live
- GET /health/ready
- GET /openapi/{documentName}.json
- GET /swagger

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
- [x] Implement `Idempotency-Key` handling cho create shipment (Redis + DB unique)
- [ ] Store request hash + response snapshot (tùy scope)
- Acceptance:
  - Gửi same key -> trả same response, không tạo record mới
- Partial: `CreateShipment` requires `Idempotency-Key`; Redis + DB idempotency lookup and a unique DB constraint are implemented.
- Missing: request hash persistence and response snapshot storage.
- Evidence: `src/Host/Logistics.Api.Host/Controllers/Shipments/ShipmentsController.cs`; `src/Services/Shipments/Logistics.Api.Shipments.Application/Commands/CreateShipment/CreateShipmentCommandHandler.cs`; `src/Services/Shipments/Logistics.Api.Shipments.Infrastructure/Idempotency/RedisIdempotencyService.cs`

### A5. Rate limiting policies (P0)
- [x] Global fixed window (đã có)
- [ ] Add policy by endpoint group: `shipments-create`, `tracking-public`
- Acceptance:
  - 429 có ProblemDetails + retry-after header (nếu có)
- Partial: global fixed-window rate limiting is configured at host level.
- Missing: endpoint-group policies and explicit retry-after behavior.
- Evidence: `src/Host/Logistics.Api.Host/Program.cs`; `src/Host/Logistics.Api.Host/Extensions/RateLimitingExtensions.cs`

### A6. Health checks hardening (P0)
- [x] readiness/liveness endpoints chuẩn
- [ ] Add `/metrics` Prometheus
- Acceptance:
  - Docker local up -> health reports dependencies
- Partial: readiness/liveness endpoints plus dependency health checks are mapped.
- Missing: `/metrics` endpoint.
- Evidence: `src/Host/Logistics.Api.Host/Extensions/HealthChecksExtensions.cs`; `src/Host/Logistics.Api.Host/Program.cs`

### A7. OpenTelemetry spans enrichment (P1)
- [ ] Add custom spans tags: correlationId, merchantId
- [x] Export to Jaeger (already)
- Acceptance:
  - Trace shows route + status code + correlationId
- Partial: OpenTelemetry tracing exports via OTLP.
- Missing: custom span tags for `correlationId` and `merchantId`.
- Evidence: `src/BuildingBlocks/Logistics.Api.BuildingBlocks.Observability/Observability/OpenTelemetry/OpenTelemetryBootstrapper.cs`; `src/Host/Logistics.Api.Host/Program.cs`

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
- Partial: merchant-user relation schema exists.
- Missing: application/API flow to create or manage merchant-user links.
- Evidence: `src/Services/Merchants/Logistics.Api.Merchants.Infrastructure/Persistence/Configurations/MerchantUserEntityConfiguration.cs`; `src/Services/Merchants/Logistics.Api.Merchants.Infrastructure/Migrations/20260323033509_InitialMerchantsSchema.cs`

---

## D. Pricing

### D1. PricingRule model + fee calculator (P0) ✅
- [x] DB table `pricing.pricing_rules` + migration (`InitialPricingSchema`)
- [x] Implement calculator:
  - zone type (same province / national) via `ZoneResolver`
  - weight bracket (`MinWeightGram` / `MaxWeightGram`)
  - COD fee percent (`CodFeePercent`)
  - service type (`ServiceType` enum)
- [x] `IPricingCalculator` / `PricingCalculator` in Application layer
- [x] Unit tests: `PricingRuleTests`, `ZoneResolverTests`, `PricingCalculatorTests` (37 passing)
- [x] Host: `AddPricingModule` registered
- Note: CreateShipment integration (fee fields on response) is part of task E2

---

## E. Shipments (Core Domain)

### E1. Shipment aggregate + status machine (P0)
- [x] Domain model: Shipment + TrackingEvent
- [x] Define allowed transitions table (state machine)
- Acceptance:
  - Invalid transition -> 409 Conflict (ProblemDetails)
- ✅ Done: `Shipment` aggregate and `TrackingEvent` in Shipments.Domain with `ShipmentStateMachine` transition rules.

### E2. Create shipment command (P0)
- [x] Endpoint `POST /api/v1/shipments`
- [x] Idempotency-Key required
- [x] Generate trackingCode + shipmentCode
- [x] Persist shipment + tracking event "Created"
- Acceptance:
  - Duplicate request with same idempotency key returns same result
- ✅ Done: `ShipmentsController.CreateShipment`, `CreateShipmentCommandHandler`, `CreateShipmentCommandValidator`, `ShipmentsDbContext`, and shipments migrations implement the full flow.

### E3. Cancel shipment (P0)
- [ ] Endpoint `POST /api/v1/shipments/{id}/cancel`
- [ ] Only allowed in certain states
- [ ] Append tracking event "Cancelled"
- Acceptance:
  - Cancelled shipments not movable
- Partial: generic shipment status transition supports `Cancelled` and appends tracking events.
- Missing: dedicated cancel endpoint, cancel-specific command/validator, and dedicated API contract implementation.
- Evidence: `src/Services/Shipments/Logistics.Api.Shipments.Domain/Services/ShipmentStateMachine.cs`; `src/Services/Shipments/Logistics.Api.Shipments.Domain/Entities/Shipment.cs`; `src/Host/Logistics.Api.Host/Controllers/Shipments/ShipmentStatusTransitionsController.cs`

### E4. Status transitions (P0)
- [x] Endpoint `POST /api/v1/shipments/{id}/status-transitions`
- [x] Append tracking event
- [x] Publish integration event (via outbox)
- Acceptance:
  - Each status change yields new tracking event row
- ✅ Done: `ShipmentStatusTransitionsController`, `TransitionShipmentStatusCommandHandler`, `TrackingEventConfiguration`, and outbox writer/migrations cover the full flow.

### E5. Shipment query APIs (P0)
- [ ] `GET /api/v1/shipments/{id}`
- [ ] `GET /api/v1/shipments/by-tracking/{trackingCode}`
- [ ] `GET /api/v1/shipments` (filters, paging)
- Acceptance:
  - Merchant scope filter enforced
- Partial: repository-level shipment lookup primitives exist.
- Missing: shipment query controllers/routes, query handlers, and merchant-scoped paging API.
- Evidence: `src/Services/Shipments/Logistics.Api.Shipments.Domain/Repositories/IShipmentRepository.cs`; `src/Services/Shipments/Logistics.Api.Shipments.Infrastructure/Repositories/ShipmentRepository.cs`

---

## F. Tracking

### F1. Public tracking lookup (P0)
- [x] `GET /api/v1/tracking/{trackingCode}`
- [x] `GET /api/v1/tracking/{trackingCode}/timeline`
- Acceptance:
  - No auth required, rate limit stricter
- ✅ Done: `TrackingController`, `GetTrackingSummaryQueryHandler`, and `GetTrackingTimelineQueryHandler` are implemented on top of the shipments aggregate/migration.

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
- [x] Define events in `BuildingBlocks.Contracts`
- [x] Envelope fields: eventId, correlationId, occurredOn, version
- Acceptance:
  - Stable schema + versioning
- ✅ Done: shipment and webhook events inherit from `IntegrationEventEnvelope<TPayload>` in BuildingBlocks.Contracts.

### H2. Outbox publisher worker (P0)
- [x] Poll outbox table, publish RabbitMQ, mark processed
- [x] Retry on transient failures + backoff
- Acceptance:
  - Publish is at-least-once
- ✅ Done: outbox table/migrations, `OutboxPublisherBackgroundService`, and worker DI/MassTransit registration are implemented.

### H3. Inbox consumer idempotency (P0)
- [ ] Consumers record processed message ids
- Acceptance:
  - Duplicate broker deliveries do not duplicate side effects
- Partial: inbox persistence and idempotency filter exist.
- Missing: consumer registration that applies the inbox filter.
- Evidence: `src/Services/Notifications/Logistics.Api.Notifications.Infrastructure/Messaging/InboxIdempotencyFilter.cs`; `src/Services/Notifications/Logistics.Api.Notifications.Infrastructure/Persistence/Configurations/InboxMessageConfiguration.cs`

### H4. DLQ strategy (P1)
- [ ] Configure RabbitMQ dead-letter exchange/queue
- [ ] Consumer failure -> retry then route to DLQ
- Acceptance:
  - Operator can inspect failed messages

---

## I. Webhooks / Notifications

### I1. Webhook subscription APIs (P0)
- [x] CRUD subscription for merchant
- [x] Store secret, callback url, events
- Acceptance:
  - Validate URL, event types
- ✅ Done: `WebhookSubscriptionsController`, webhook command/query handlers, validators, `NotificationsDbContext`, and notifications migration are implemented.

### I2. Webhook delivery worker + retry (P0)
- [x] Persist deliveries
- [x] Retry exponential backoff
- [x] HMAC signature
- Acceptance:
  - Delivery logs include request/response; idempotent on eventId
- Partial: deliveries are persisted, retried, logged, and signed.
- Missing: DI/runtime registration for the notification consumers and `WebhookDeliveryBackgroundService`.
- Evidence: `src/Services/Notifications/Logistics.Api.Notifications.Infrastructure/Messaging/ShipmentCreatedIntegrationEventConsumer.cs`; `src/Services/Notifications/Logistics.Api.Notifications.Infrastructure/Messaging/ShipmentStatusChangedIntegrationEventConsumer.cs`; `src/Services/Notifications/Logistics.Api.Notifications.Infrastructure/Services/WebhookDeliveryBackgroundService.cs`

### I3. Webhook test endpoint (P1)
- [ ] Send test payload to merchant
- Acceptance:
  - Useful for merchant integration

---

## J. Search (Elasticsearch)

### J1. Shipment search index mapping (P0)
- [x] Define index settings/mappings
- [x] Create index on startup (dev only) or via migration script
- Acceptance:
  - `trackingCode` keyword; names text; dates range
- ✅ Done: Elasticsearch mappings/settings are defined and startup index initialization is implemented.

### J2. Indexing consumer (P0)
- [x] Consume ShipmentCreated/StatusChanged, update ES doc
- Acceptance:
  - ES eventually consistent within seconds
- ✅ Done: search consumers reindex shipments and are registered in `Logistics.Api.Messaging.Worker`.

### J3. Search API (P0)
- [x] `GET /api/v1/search/shipments`
- [x] filters + paging + sorting
- Acceptance:
  - Queries use ES, not PostgreSQL
- ✅ Done: `SearchController`, `SearchShipmentsQueryHandler`, `SearchShipmentsQueryValidator`, and `ElasticsearchShipmentSearchService` implement the API on Elasticsearch.

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
- [x] Valid transitions pass
- [x] Invalid transitions fail
- Acceptance:
  - Coverage for core logic
- ✅ Done: `tests/Logistics.Api.UnitTests/Shipments/ShipmentStateMachineTests.cs` covers valid and invalid transitions plus terminal states.

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
- Partial: API contract covers the implemented auth, shipment create/status, tracking, webhook subscription, and search endpoints.
- Missing: shipment detail/by-tracking/cancel endpoints remain documented but unimplemented; operational endpoints are undocumented; `docs/db-schema.md` is not updated for the audited schema.
- Evidence: `docs/api-contract.md`; `src/Host/Logistics.Api.Host/Controllers`; `src/Host/Logistics.Api.Host/Extensions`