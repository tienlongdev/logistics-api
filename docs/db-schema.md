# Database Schema — Logistics API (Draft v1)

> PostgreSQL là **source of truth**. Elasticsearch chỉ phục vụ search/read model.

## 1. Naming & conventions
- Schema per module: `identity`, `merchants`, `shipments`, `hubs`, `notifications`, `pricing`, `reconciliation`, `messaging`
- Timestamps: `created_at`, `updated_at` dùng `timestamptz`
- Primary key: `uuid` (application-generated hoặc DB default)
- Uniqueness/invariants enforced bằng unique indexes
- Không lưu plain secrets/tokens: chỉ lưu **hash** (hoặc encrypted-at-rest cho secrets)

## 2. Schemas
```sql
CREATE SCHEMA identity;
CREATE SCHEMA merchants;
CREATE SCHEMA shipments;
CREATE SCHEMA tracking;        -- optional, có thể gộp vào shipments
CREATE SCHEMA hubs;
CREATE SCHEMA notifications;
CREATE SCHEMA pricing;
CREATE SCHEMA reconciliation;
CREATE SCHEMA messaging;
```

## 3. Tables — Identity

### 3.1 `identity.users`
- **PK**: `id`
- **Unique**: `email`
- **Indexes**: `email`, `phone`

Columns:
- `id uuid`
- `email varchar(255) not null unique`
- `phone varchar(20) null`
- `full_name varchar(255) not null`
- `password_hash varchar(512) not null`
- `is_active boolean not null default true`
- `email_verified boolean not null default false`
- `created_at timestamptz not null`
- `updated_at timestamptz not null`

Query patterns:
- Login lookup by `email`
- Admin search by `phone`

### 3.2 `identity.roles`
- **PK**: `id`
- **Unique**: `name`

Columns:
- `id uuid`
- `name varchar(100) not null unique` (Admin, Operator, HubStaff, Merchant)
- `description varchar(500) null`

### 3.3 `identity.user_roles`
- **PK**: `(user_id, role_id)`
- **FK**:
  - `user_id -> identity.users(id)` (cascade delete)
  - `role_id -> identity.roles(id)` (cascade delete)

Columns:
- `user_id uuid not null`
- `role_id uuid not null`
- `granted_at timestamptz not null`

Indexes:
- PK composite đã đủ cho join

### 3.4 `identity.refresh_tokens`
- **PK**: `id`
- **Unique**: `token_hash`
- **Indexes**:
  - `user_id`
  - `expires_at` (optionally partial index where `revoked_at is null`)

Columns:
- `id uuid`
- `user_id uuid not null` (FK -> identity.users)
- `token_hash varchar(512) not null unique` (SHA-256/BCrypt tuỳ design)
- `expires_at timestamptz not null`
- `revoked_at timestamptz null`
- `revoked_reason varchar(255) null`
- `ip_address varchar(50) null`
- `user_agent text null`
- `created_at timestamptz not null`

Query patterns:
- Refresh token validation by `token_hash` + `expires_at` + `revoked_at is null`
- Revoke all tokens by `user_id`

---

## 4. Tables — Merchants

### 4.1 `merchants.merchants`
- **PK**: `id`
- **Unique**: `code`, `email`
- **Indexes**: `code`, `api_key_prefix`

Columns:
- `id uuid`
- `code varchar(20) not null unique` (vd: MCH0001)
- `name varchar(255) not null`
- `tax_code varchar(50) null`
- `email varchar(255) not null unique`
- `phone varchar(20) null`
- `address text null`
- `api_key varchar(512) null` (hash)
- `api_key_prefix varchar(10) null` (vd: lga_)
- `webhook_secret varchar(512) null` (hash/encrypted)
- `settings jsonb null`
- `is_active boolean not null default true`
- `created_at timestamptz not null`
- `updated_at timestamptz not null`

### 4.2 `merchants.merchant_users`
- **PK**: `id`
- **Unique**: `(merchant_id, user_id)`
- **FK**:
  - `merchant_id -> merchants.merchants(id)`
  - `user_id` là **logical FK** tới `identity.users(id)` (khi tách DB per service)

Columns:
- `id uuid`
- `merchant_id uuid not null`
- `user_id uuid not null`
- `role_in_merchant varchar(50) not null` (Owner/Staff/Viewer)
- `created_at timestamptz not null`

Indexes:
- `(merchant_id, user_id)` unique
- `user_id` index để lookup merchants của user

---

## 5. Tables — Shipments & Tracking (Core)

### 5.1 `shipments.shipments`
- **PK**: `id`
- **Unique**:
  - `tracking_code`
  - `shipment_code`
  - `idempotency_key` (unique để enforce idempotent create)
- **Indexes**:
  - `merchant_id`
  - `tracking_code`
  - `current_status`
  - `receiver_phone`
  - `created_at desc`
  - `(merchant_id, current_status)` composite

Columns (core):
- `id uuid`
- `tracking_code varchar(20) not null unique`
- `shipment_code varchar(30) not null unique`
- `idempotency_key varchar(255) null unique`
- `merchant_id uuid not null`
- `merchant_code varchar(20) not null`

Sender:
- `sender_name varchar(255) not null`
- `sender_phone varchar(20) not null`
- `sender_address text not null`
- `sender_province varchar(100) null`
- `sender_district varchar(100) null`
- `sender_ward varchar(100) null`

Receiver:
- `receiver_name varchar(255) not null`
- `receiver_phone varchar(20) not null`
- `receiver_address text not null`
- `receiver_province varchar(100) null`
- `receiver_district varchar(100) null`
- `receiver_ward varchar(100) null`

Package:
- `weight_gram int not null`
- `length_cm int null`
- `width_cm int null`
- `height_cm int null`
- `package_description text null`
- `declared_value numeric(15,2) not null`

Financial:
- `cod_amount numeric(15,2) not null`
- `shipping_fee numeric(15,2) not null`
- `insurance_fee numeric(15,2) not null`
- `total_fee numeric(15,2) not null`

Service:
- `service_type varchar(50) not null` (Standard/Express)
- `delivery_note text null`

Status & hub:
- `current_status varchar(50) not null`
- `current_hub_id uuid null`
- `current_hub_code varchar(20) null`

Lifecycle:
- `expected_delivery date null`
- `actual_delivered_at timestamptz null`
- `cancelled_at timestamptz null`
- `cancelled_reason text null`

Metadata:
- `created_at timestamptz not null`
- `updated_at timestamptz not null`

Query patterns:
- Merchant list shipments theo date range + status
- Lookup theo `tracking_code`
- Lookup theo `receiver_phone`
- Admin/operator filter theo merchant_code + status

### 5.2 `shipments.tracking_events` (aka shipment_status_histories)
- **PK**: `id`
- **FK**: `shipment_id -> shipments.shipments(id)`
- **Indexes**:
  - `shipment_id`
  - `tracking_code`
  - `occurred_at desc`

Columns:
- `id uuid`
- `shipment_id uuid not null`
- `tracking_code varchar(20) not null`
- `from_status varchar(50) null`
- `to_status varchar(50) not null`
- `hub_id uuid null`
- `hub_code varchar(20) null`
- `location varchar(255) null`
- `note text null`
- `operator_id uuid null`
- `operator_name varchar(255) null`
- `source varchar(50) null` (API/WORKER/SYSTEM/SCAN)
- `occurred_at timestamptz not null`
- `created_at timestamptz not null`

---

## 6. Tables — Hubs/Warehouses

### 6.1 `hubs.hubs`
- **PK**: `id`
- **Unique**: `code`
- **Indexes**: `province`

Columns:
- `id uuid`
- `code varchar(20) not null unique` (HUB-HCM-001)
- `name varchar(255) not null`
- `hub_type varchar(50) not null` (OriginHub/TransitHub/DestinationHub/DeliveryHub)
- `province varchar(100) null`
- `district varchar(100) null`
- `address text null`
- `phone varchar(20) null`
- `is_active boolean not null default true`
- `created_at timestamptz not null`

### 6.2 `hubs.hub_shipments` (optional trong MVP)
- Purpose: audit assignment history shipment ↔ hub
- **Indexes**: `hub_id`, `shipment_id`, `tracking_code`

Columns:
- `id uuid`
- `hub_id uuid not null`
- `shipment_id uuid not null` (logical FK to shipments)
- `tracking_code varchar(20) not null`
- `assignment_type varchar(50) null`
- `assigned_at timestamptz not null`
- `departed_at timestamptz null`
- `note text null`

---

## 7. Tables — Notifications/Webhooks

### 7.1 `notifications.webhook_subscriptions`
- **PK**: `id`
- **Indexes**: `merchant_id`

Columns:
- `id uuid`
- `merchant_id uuid not null`
- `callback_url varchar(2048) not null`
- `events text[] not null`
- `secret varchar(512) null` (encrypted/hashed)
- `is_active boolean not null default true`
- `created_at timestamptz not null`
- `updated_at timestamptz not null`

### 7.2 `notifications.webhook_deliveries`
- **PK**: `id`
- **Unique**: `(subscription_id, event_id)` để idempotent delivery
- **Indexes**:
  - `(status, next_retry_at)` partial where status in (Pending, Failed)
  - `merchant_id`

Columns:
- `id uuid`
- `subscription_id uuid not null`
- `merchant_id uuid not null`
- `event_type varchar(100) not null`
- `event_id uuid not null`
- `payload jsonb not null`

Delivery state:
- `status varchar(50) not null` (Pending/Success/Failed/Exhausted)
- `attempt_count int not null`
- `max_attempts int not null`
- `next_retry_at timestamptz null`
- `delivered_at timestamptz null`

Response logs:
- `last_response_code int null`
- `last_response_body text null`
- `last_error text null`

Metadata:
- `created_at timestamptz not null`
- `updated_at timestamptz not null`

---

## 8. Tables — Pricing

### 8.1 `pricing.pricing_rules`
- Rule engine đơn giản: match theo weight + zone + serviceType + COD
- **Indexes**: `(service_type, is_active)`, `priority`

Columns:
- `id uuid`
- `name varchar(255) not null`
- `service_type varchar(50) not null`
- `from_province varchar(100) null`
- `to_province varchar(100) null`
- `zone_type varchar(50) null` (SameProvince/National/NearBy)
- `min_weight_gram int not null`
- `max_weight_gram int null`
- `base_fee numeric(15,2) not null`
- `per_kg_fee numeric(15,2) not null`
- `cod_fee_percent numeric(5,2) not null`
- `is_active boolean not null`
- `effective_from date not null`
- `effective_to date null`
- `priority int not null`
- `created_at timestamptz not null`

---

## 9. Tables — Messaging (Outbox/Inbox/Audit)

### 9.1 `messaging.outbox_messages`
- **PK**: `id`
- **Indexes**: `(status, occurred_on)` where status = Pending

Columns:
- `id uuid`
- `correlation_id uuid null`
- `type varchar(512) not null`
- `payload jsonb not null`
- `occurred_on timestamptz not null`
- `processed_on timestamptz null`
- `retry_count int not null`
- `error text null`
- `status varchar(50) not null` (Pending/Processed/Failed)

### 9.2 `messaging.inbox_messages`
- **PK**: `id` (messageId từ broker) hoặc composite `(id, consumer_name)`
- Purpose: consumer idempotency

Columns:
- `id uuid`
- `consumer_name varchar(255) not null`
- `type varchar(512) not null`
- `payload jsonb not null`
- `received_on timestamptz not null`
- `processed_on timestamptz null`
- `error text null`

### 9.3 `messaging.audit_logs`
- **PK**: `id`
- **Indexes**: `(entity_type, entity_id)`, `user_id`, `occurred_at desc`

Columns:
- `id uuid`
- `correlation_id uuid null`
- `user_id uuid null`
- `user_name varchar(255) null`
- `action varchar(255) not null`
- `entity_type varchar(255) null`
- `entity_id varchar(255) null`
- `old_values jsonb null`
- `new_values jsonb null`
- `ip_address varchar(50) null`
- `user_agent text null`
- `occurred_at timestamptz not null`

---

## 10. Tables — Reconciliation/COD

### 10.1 `reconciliation.cod_transactions`
- **Indexes**: `(merchant_id, status)`, `shipment_id`

Columns:
- `id uuid`
- `shipment_id uuid not null`
- `tracking_code varchar(20) not null`
- `merchant_id uuid not null`
- `cod_amount numeric(15,2) not null`
- `status varchar(50) not null` (Pending/Collected/Remitted/Disputed)
- `collected_at timestamptz null`
- `remitted_at timestamptz null`
- `remittance_id uuid null`
- `note text null`
- `created_at timestamptz not null`
- `updated_at timestamptz not null`