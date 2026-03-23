# Roadmap — Logistics API MVP

## Milestone 0 — Repo & Local Stack (Done)
- Bootstrap solution structure
- Docker compose infra
- Observability foundation

## Milestone 1 — Identity/Auth (P0)
- Auth login/refresh/logout
- RBAC policies
- Seed roles/admin

## Milestone 2 — Shipments Core (P0)
- Shipment aggregate + tracking events
- Create shipment (idempotent)
- Cancel shipment
- Status transitions
- Tracking API public

## Milestone 3 — Messaging (P0)
- Integration events contracts
- Outbox publisher worker
- Inbox idempotency

## Milestone 4 — Webhooks (P0)
- Subscriptions
- Delivery worker + retry + signature
- Delivery logs

## Milestone 5 — Search (P0)
- ES index + mapping
- Indexing consumer
- Search API

## Milestone 6 — Pricing & COD (P1)
- Pricing rules engine
- COD transactions + reconciliation summary

## Milestone 7 — Hardening (P1)
- Metrics dashboards
- DLQ + operational tooling
- Integration tests + architecture tests
- Documentation + API collection