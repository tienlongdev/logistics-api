# 🚚 Logistics API (.NET 10)

Enterprise-grade backend cho hệ thống logistics/shipping (mô phỏng Viettel Post / GHN), được thiết kế theo hướng **production-ready**, **observability-first** và **microservices-ready**.

---

## ✨ Overview

Đây là một backend system xử lý các nghiệp vụ logistics:

* Quản lý đơn hàng (Shipments)
* Theo dõi trạng thái (Tracking)
* Tính giá (Pricing)
* Thông báo (Notifications)
* Tìm kiếm (Search)
* Đối soát (Reconciliation)

Hệ thống được xây dựng theo hướng:

> **Modular Monolith → Microservices Evolution**

Cho phép:

* phát triển nhanh ở phase đầu
* scale linh hoạt khi hệ thống lớn lên

---

## 🧱 Tech Stack

### Core

* **.NET 10** (ASP.NET Core Web API)
* **PostgreSQL** — source of truth
* **Redis** — cache, idempotency, rate limiting primitives
* **RabbitMQ** — integration events

### Observability

* **Serilog + Seq** — structured logging
* **OpenTelemetry + Jaeger** — distributed tracing
* **Prometheus + Grafana** — metrics & monitoring

### Search & Data

* **Elasticsearch** — full-text search

### DevOps

* **Docker Compose** — local infrastructure stack

---

## 🏗 Architecture

### Modular Monolith (Phase 1)

* Chạy **single process**
* Tách module theo **bounded context**
* Dependency direction theo **Clean Architecture**

### Microservices-ready (Phase 2)

* Modules có thể tách thành service độc lập
* Giao tiếp qua:

    * **Integration Events (RabbitMQ)**
    * **Outbox Pattern**

---

## 📦 Solution Structure

```text
src/
  BuildingBlocks/        ← cross-cutting (observability, messaging, etc.)
  Services/
    Identity/
    Merchants/
    Shipments/
    Tracking/
    Hubs/
    Pricing/
    Notifications/
    Search/
    Reconciliation/
  Host/                  ← API composition root

tests/
docker/
docker-compose.yml
```

---

## 🔍 Observability (First-class Citizen)

Hệ thống được thiết kế với **observability là nền tảng**, không phải add-on.

### Capabilities

* **Correlation ID**

    * Trace một request xuyên suốt hệ thống
* **Structured Logging**

    * Log có metadata (CorrelationId, Application, Environment)
* **Distributed Tracing**

    * Visualize flow trên Jaeger
* **Metrics**

    * Latency, throughput, error rate

### Example

```text
Client → API → Shipments → Pricing → DB → RabbitMQ → Tracking → Notifications
```

Tất cả được liên kết bằng:

```text
CorrelationId=abc123
```

---

## 🔄 Request Flow (Simplified)

```text
HTTP Request
  ↓
[Correlation Middleware]
  ↓
[Logging / Tracing Context]
  ↓
[Application Modules]
  ↓
[Database / Message Bus]
```

---

## 🚀 Getting Started

### 1. Clone repository

```bash
git clone https://github.com/your-repo/logistics-api.git
cd logistics-api
```

---

### 2. Start infrastructure

```bash
docker-compose up -d
```

Services:

* PostgreSQL
* Redis
* RabbitMQ
* Seq
* Jaeger
* Elasticsearch
* Prometheus / Grafana

---

### 3. Run API

```bash
dotnet run --project src/Host/Logistics.Api.Host
```

---

### 4. Access tools

| Tool    | URL                    |
| ------- | ---------------------- |
| API     | http://localhost:5080  |
| Seq     | http://localhost:5341  |
| Jaeger  | http://localhost:16686 |
| Grafana | http://localhost:3000  |

### 5. API Docs

* Scalar: `/scalar/v1`
* Swagger: `/swagger`
* OpenAPI JSON: `/openapi/v1.json`

---

## 🧠 Design Principles

* **Separation of Concerns**
* **Clean Architecture**
* **Event-driven communication**
* **Observability-first**
* **Production-ready mindset**
* **Microservices evolution strategy**

---

## 🔐 Future Work

* Authentication / Authorization (JWT, Identity)
* Full Outbox + Inbox pattern
* Saga / orchestration for complex workflows
* Multi-tenant support
* Advanced rate limiting
* CI/CD pipeline
* Kubernetes deployment

---

## 📌 Key Concepts

| Concept          | Description                            |
| ---------------- | -------------------------------------- |
| CorrelationId    | Trace business flow (logs)             |
| TraceId          | Distributed tracing (OpenTelemetry)    |
| Outbox Pattern   | Reliable event publishing              |
| Modular Monolith | Structured single-process architecture |

---

## 🧾 License

MIT (or your preferred license)

---

## 💬 Summary

> Đây là một backend logistics được thiết kế theo chuẩn production, với observability là nền tảng và kiến trúc sẵn sàng scale lên microservices.

Nếu Domain trả lời:

> “Hệ thống làm gì?”

thì hệ thống này đảm bảo bạn luôn trả lời được:

> “Nó đang làm gì, và đang lỗi ở đâu?”
