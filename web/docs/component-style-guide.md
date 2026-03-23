# Component Style Guide

## Direction

- Visual tone: modern SaaS, neutral surfaces, mot primary accent duy nhat
- Layout tone: generous spacing, rounded panels, subtle glass surface, khong dung decoration qua da
- Motion: chi dung fade-up va shimmer cho state transitions co y nghia

## Tokens

- Nguon token dung chung: `styles/design-tokens.css`
- Semantic colors bat buoc dung qua token: `background`, `foreground`, `card`, `muted`, `primary`, `accent`, `destructive`, `success`
- Khong hard-code mau vao component moi tru khi dang prototyping

## Accessibility

- Moi button/icon button phai co accessible name
- Focus ring dung token `ring`
- Sidebar, drawer, form field deu phai keyboard reachable
- Error copy canh tranh ro, khong dua loi ky thuat tho ra UI neu khong can thiet

## Composition

- Page layout dung `page-shell` va `page-header`
- Panel dung `Card` hoac `surface-panel`
- Data state dung bo `PageLoadingState`, `EmptyState`, `ErrorState`
- Form moi dung `react-hook-form` + `zod` cho parse va validate

## Data patterns

- Query keys phai co filter va auth scope lien quan
- Contract map vao `lib/types/api.ts`
- Fetcher tap trung trong `lib/api/client.ts`
- Khi backend chua san sang, chi fallback mock cho route preview da dinh san

## Adding new components

- Uu tien shadcn-style primitives trong `components/ui`
- Component complex nen dung props ro nghia, khong truyền object vo danh qua lon
- Neu component co variant, dung `class-variance-authority`