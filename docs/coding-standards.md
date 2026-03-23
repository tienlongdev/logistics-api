# Coding Standards — Logistics API (.NET 10)

## 1. General principles
- Production-ready, maintainable, testable
- Không nhét business logic vào Controller
- Validate input ở Application layer (FluentValidation)
- Domain model giữ invariants; tracking timeline là first-class
- Logs/traces/metrics phải đủ để vận hành (correlation id everywhere)

## 2. Project structure rules
- Mỗi module có 3 layers:
  - `*.Domain`: Entities, VOs, domain services, domain events
  - `*.Application`: Commands/Queries/Handlers, DTOs, validators, orchestrations
  - `*.Infrastructure`: EF DbContext, persistence, external clients, message bus adapters
- `Host` chỉ wiring, middleware, module registration, controllers/endpoints

## 3. Naming conventions
- Namespace: `Logistics.Api.<Module>.<Layer>`
- Command/Query:
  - `CreateShipmentCommand`
  - `GetShipmentByTrackingCodeQuery`
- Handler:
  - `CreateShipmentCommandHandler`
- DTO:
  - `ShipmentDto`, `CreateShipmentRequest`
- Errors:
  - `ShipmentErrors.InvalidStateTransition`

## 4. Error handling
- Application handlers return `Result` / `Result<T>` (khuyến nghị)
- Exceptions chỉ dùng cho truly exceptional (infra failures, programmer errors)
- API trả `ProblemDetails` chuẩn

## 5. Validation
- FluentValidation
- Không rely vào DataAnnotations cho business validation
- Validation errors map ra ProblemDetails với details per field

## 6. Logging
- Log structured (Serilog)
- Không log raw secrets/token/password
- Redact PII nếu cần (roadmap)
- CorrelationId phải luôn có trong log context

## 7. Database & EF Core
- Migrations-only
- Không dùng `EnsureCreated`
- Không disable tracking bừa bãi; query read-model có thể dùng `AsNoTracking()`
- Index và constraints phải reflect business invariants
- Soft delete chỉ dùng khi thật cần (cân nhắc)

## 8. Messaging
- Publish integration events qua Outbox (không publish trực tiếp sau SaveChanges)
- Consumers phải idempotent (Inbox table hoặc equivalent)
- Retry + DLQ strategy rõ ràng

## 9. Webhook rules
- Persist delivery attempt results
- Retry exponential backoff
- HMAC signature
- Timeouts + circuit breaker (Polly) khi gọi merchant

## 10. Testing
- Unit tests cho domain/application logic
- Integration tests dùng Testcontainers
- Architecture tests để enforce dependency rules

## 11. Pull request guidelines (nếu dùng)
- Mỗi PR/commit tập trung 1 chủ đề (small, reviewable)
- Update docs nếu có thay đổi contract/schema
- Không merge nếu build/test fail