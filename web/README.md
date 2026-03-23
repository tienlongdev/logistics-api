# Logistics Web

Frontend dashboard moi cho repo logistics, duoc dat rieng tai `/web` va khong can sua backend hien tai.

## Stack

- Next.js App Router
- TypeScript
- Tailwind CSS
- shadcn/ui + lucide-react
- Zustand
- TanStack Query
- react-hook-form + zod
- next-themes

## Features trong scaffold

- Responsive app shell: desktop sidebar, mobile drawer
- Auth gate placeholder + login form map theo `POST /api/v1/auth/login`
- Routes: `/login`, `/dashboard`, `/shipments`, `/tracking`, `/search`, `/settings`
- Loading, empty, error, toast states
- Shared design tokens trong `styles/design-tokens.css`
- Component style guide trong `docs/component-style-guide.md`
- UI guidelines chi tiet trong `docs/ui-guidelines.md`

## Environment

Tao file `.env.local` tu `.env.example` neu can override:

```bash
NEXT_PUBLIC_API_BASE_URL=http://localhost:8080/api/v1
NEXT_PUBLIC_ENABLE_MOCK_FALLBACK=true
```

## Development

Repo may chua co `pnpm` global. Neu macOS cua ban co `corepack`, co the dung:

```bash
corepack enable
cd web
corepack pnpm install
corepack pnpm dev
```

Neu da co `pnpm` san, dung:

```bash
cd web
pnpm install
pnpm dev
```

## Scripts

```bash
cd web
corepack pnpm lint
corepack pnpm build
corepack pnpm test:e2e
```

## Docker

Build image:

```bash
docker build -t logistics-web ./web
docker run --rm -p 3000:3000 \
	-e NEXT_PUBLIC_API_BASE_URL=http://host.docker.internal:5080/api/v1 \
	logistics-web
```

Compose:

```bash
docker compose -f web/compose.yaml up --build
```

Compose file da map `NEXT_PUBLIC_API_BASE_URL` toi backend local mac dinh. Dieu chinh bien moi truong neu backend chay o host/port khac.

## Notes ve backend contract

- `/dashboard` va `/shipments` dung `GET /api/v1/search/shipments` lam read model de tranh sua backend.
- `/tracking` dung public tracking endpoints.
- `GET /api/v1/shipments/{id}`, `GET /api/v1/shipments/by-tracking/{trackingCode}` va `POST /api/v1/shipments/{id}/cancel` dang duoc docs mo ta nhung chua co implementation day du trong backend audit, nen frontend chi dat cho cho workflow tuong ung.
- Khi auth backend chua chay, login se fallback vao demo session de preview shell va interactions.

## QA scope da them

- Smoke E2E cho login, tracking, not-found, va shell shortcut entry
- Global loading/error/not-found states da duoc polish
- Empty states, dialogs, skeletons, va toasts da duoc chuan hoa theo `docs/ui-guidelines.md`