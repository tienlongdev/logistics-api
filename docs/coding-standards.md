# Coding Standards — Logistics API (.NET 10)

## 1. General principles
- Production-ready, maintainable, testable
- Không nhét business logic vào Controller
- Validate input ở Application layer (FluentValidation)
- Domain model giữ invariants; tracking timeline là first-class feature
- Logs/traces/metrics phải đủ để vận hành (correlation id everywhere)

## 2. Layer rules
- `*.Domain`: không reference `Infrastructure`
- `*.Application`: không reference `Infrastructure`
- `*.Infrastructure`: có thể reference Application/Domain
- `Host`: chỉ wiring; không chứa domain logic

## 3. Naming conventions
- Command/Query: `VerbNounCommand`, `GetXxxQuery`
- Handler: `XxxHandler`
- DTO: `XxxDto`, `CreateXxxRequest`, `XxxResponse`
- Errors: stable `Error.Code` (vd: `shipments.invalid_state_transition`)

## 4. Error handling
- Prefer `Result` / `Result<T>` trong Application layer
- Exceptions dành cho infra failures (DB down, network)
- API trả ProblemDetails thống nhất

## 5. Validation
- FluentValidation
- Không rely vào DataAnnotations cho business rules
- Validation errors trả 400 và chỉ rõ field

## 6. Logging & security
- Không log password/token/api key plain
- Mask/Redact PII khi cần (roadmap)
- Luôn attach `CorrelationId`

## 7. Database
- Migrations-only
- Unique index cho invariants (trackingCode, idempotencyKey)
- Dùng `AsNoTracking()` cho read models khi phù hợp

## 8. Messaging
- Publish integration events qua Outbox
- Consumer idempotent (Inbox)
- Retry + DLQ rõ ràng

## 9. Webhooks
- Persist deliveries
- Retry exponential backoff
- HMAC signature, timeout, circuit breaker

## 10. Testing
- Unit tests: domain rules, state machine
- Integration tests: Testcontainers + API end-to-end
- Architecture tests: enforce dependency rules