# BuildingBlocks.Observability

## Overview

`BuildingBlocks.Observability` là tầng **cross-cutting infrastructure** cung cấp các capability để quan sát (observe) hành vi của hệ thống thông qua:

* Correlation ID (trace flow)
* Structured logging (Serilog)
* Distributed tracing (OpenTelemetry - foundation)
* Metadata enrichment (Application, Environment, Machine, ...)

> Mục tiêu: giúp hệ thống có thể được **trace, debug và monitor trong production**.

---

## Responsibilities

### 1. Correlation (Flow Tracking)

Đảm bảo mỗi request đều có một **Correlation ID duy nhất**:

* Đọc từ header `X-Correlation-Id` nếu client gửi
* Nếu không có → tự generate
* Gắn vào:

    * `HttpContext`
    * response header
    * logging context (Serilog)

👉 Cho phép trace một business flow xuyên suốt nhiều module/service.

---

### 2. Logging (Serilog)

Chuẩn hóa logging toàn hệ thống:

* Structured logging
* Enrich metadata:

    * `CorrelationId`
    * `Application`
    * `Environment`
    * `MachineName`
* Output:

    * Console
    * Seq

👉 Giúp log có thể query, filter và phân tích dễ dàng.

---

### 3. Tracing (OpenTelemetry - foundation)

Chuẩn bị cho distributed tracing:

* ASP.NET Core instrumentation
* HttpClient instrumentation
* Export qua OTLP → Jaeger

👉 Giúp theo dõi request đi qua các component nào và mất bao lâu.

---

### 4. Configuration (Options Pattern)

Chuẩn hóa cấu hình:

* `CorrelationIdOptions`
* `OpenTelemetryOptions`
* Logging config qua `appsettings.json`

---

## Architecture Role

```text
Client
  ↓
[Correlation Middleware]   ← BuildingBlocks.Observability
  ↓
[Logging Context]          ← BuildingBlocks.Observability
  ↓
[Application / Modules]
  ↓
[Database / Message Bus]
```

---

## Example

### Request

```http
POST /api/v1/shipments
X-Correlation-Id: abc123
```

### Flow

```text
Client
  → Logistics.Api.Host
  → Shipments
  → Pricing
  → PostgreSQL
  → RabbitMQ
  → Tracking
  → Notifications
```

### Logs

```text
[INF] Creating shipment CorrelationId=abc123
[INF] Calculating price CorrelationId=abc123
[ERR] DB timeout CorrelationId=abc123
```

👉 Toàn bộ log được liên kết bằng cùng một Correlation ID.

---

## Without Observability (Problem)

```text
[INF] Creating shipment
[INF] Calculating price
[ERR] DB timeout
```

❌ Không biết log nào thuộc cùng request
❌ Không trace được flow
❌ Khó debug production

---

## Key Concepts

| Concept       | Purpose                     |
| ------------- | --------------------------- |
| CorrelationId | Trace business flow (logs)  |
| TraceId       | Distributed tracing (OTel)  |
| SpanId        | Internal operation in trace |

---

## Usage

### Register Middleware

```csharp
app.UseMiddleware<CorrelationIdMiddleware>();
```

### Configure Serilog

```csharp
Log.Logger = SerilogBootstrapper
    .CreateLoggerConfiguration(configuration, "Logistics.Api.Host")
    .CreateLogger();
```

### Enable Request Logging

```csharp
app.UseSerilogRequestLogging();
```

---

## Configuration Example

```json
{
  "Correlation": {
    "HeaderName": "X-Correlation-Id"
  },
  "Serilog": {
    "MinimumLevel": "Information",
    "WriteTo": [
      { "Name": "Console" },
      { "Name": "Seq", "Args": { "serverUrl": "http://localhost:5341" } }
    ]
  }
}
```

---

## Design Principles

* Cross-cutting concern (không chứa business logic)
* Centralized observability
* Consistent logging & tracing
* Production-first
* Microservices-ready

---

## Summary

`BuildingBlocks.Observability` là:

> Tầng nền tảng giúp bạn **nhìn thấy, hiểu và debug toàn bộ hệ thống** thông qua logs, traces và correlation.

Nếu Domain trả lời:

> “Hệ thống làm gì?”

thì Observability trả lời:

> “Hệ thống đang làm gì và đang lỗi ở đâu?”
