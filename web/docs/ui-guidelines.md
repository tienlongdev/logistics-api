# UI Guidelines

## Direction

- Giao dien uu tien neutral surfaces, mot accent cyan duy nhat, shadow tiet che, va radius nhat quan.
- Khong invent API moi. Neu backend con thieu endpoint, hien fallback ro rang thay vi gia lap feature.
- Mobile truoc, table sau. Cac danh sach can co stacked card mode tren man hinh hep.

## Forms

- Dung `react-hook-form` + `zod` cho moi form co validate.
- Label luon hien thi tren field, loi hien inline duoi field.
- Primary action dat o cuoi form, disabled khi mutation dang chay.
- Dung `Input`, `Textarea`, va select tokenized; tranh hard-code border/radius moi lan.
- Keyboard shortcuts nen ro rang va an toan. Shortcut toan app uu tien focus search thay vi trigger mutation.

## Tables

- Desktop dung bang co heading uppercase muted, hang tach bang border nhe.
- Mobile khong ep scroll ngang neu co the. Uu tien stacked cards voi thong tin chinh va action ro rang.
- Trang thai luon dung `StatusBadge` hoac `Badge` theo mapping semantic.
- Paginated lists can giu nut `Trang truoc` / `Trang sau` gan nhau va disabled state ro rang.

## Dialogs

- Moi thao tac co the gay mat context hoac doi du lieu quan trong can qua confirm dialog.
- Dung `ConfirmDialog` cho logout, destructive actions, va shipment status updates.
- Dialog title can mo ta hanh dong, description can noi ro he qua.
- Nut confirm dung `destructive` tone neu hanh dong kho hoan tac.

## Skeletons

- Dung `PageLoadingState` theo variant: `dashboard`, `list`, `detail`, `tracking`.
- Skeleton can phan anh bo cuc that cua man hinh, khong dung mot block lon vo nghia.
- Loading route-level dat trong `app/loading.tsx` va `app/(app)/loading.tsx`.

## Empty States

- Dung `EmptyState` voi illustration inline SVG nhe, title ngan, description huong dan ro.
- Chon `variant` phu hop: `search`, `tracking`, `shipments`, `generic`.
- Chi them CTA khi nguoi dung co hanh dong tiep theo hop ly.

## Toasts

- Error API mac dinh duoc toast tu `lib/api/client.ts`.
- Success toast chi them cho action co y nghia: login, logout, status update, luu local settings.
- Toast copy ngan, mo ta ket qua thay vi repeat ten button.

## Resilience

- Route error, not-found, va backend-gap state phai giai thich ro nguon van de.
- Khong dua stack trace ra UI. Chi hien digest hoac huong dan troubleshooting gon.
- Khi endpoint backend chua ton tai, dung `BackendNotAvailable` thay vi fake data moi.