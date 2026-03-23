import { z } from "zod";

import { shipmentStatuses } from "@/lib/types/api";

const shipmentStatusSchema = z.enum(shipmentStatuses);

export const searchFiltersSchema = z
  .object({
    trackingCode: z.string().optional(),
    shipmentCode: z.string().optional(),
    merchantCode: z.string().optional(),
    receiverPhone: z.string().optional(),
    status: z.union([z.literal(""), shipmentStatusSchema]).optional(),
    fromDate: z.string().optional(),
    toDate: z.string().optional(),
    sort: z.string().default("updatedAt:desc"),
  })
  .refine(
    (value) => !value.fromDate || !value.toDate || value.fromDate <= value.toDate,
    {
      message: "Ngày kết thúc phải lớn hơn hoặc bằng ngày bắt đầu.",
      path: ["toDate"],
    },
  );

export type SearchFiltersSchema = z.infer<typeof searchFiltersSchema>;

export const trackingLookupSchema = z.object({
  trackingCode: z.string().min(5, "Vui lòng nhập mã vận đơn hợp lệ."),
});

export type TrackingLookupSchema = z.infer<typeof trackingLookupSchema>;

export const settingsSchema = z.object({
  displayName: z.string().min(2, "Tên hiển thị phải có ít nhất 2 ký tự."),
  emailDigest: z.boolean().default(true),
  compactDensity: z.boolean().default(false),
});

export type SettingsSchema = z.infer<typeof settingsSchema>;