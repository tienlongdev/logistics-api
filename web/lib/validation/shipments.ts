import { z } from "zod";

import { shipmentStatuses } from "@/lib/types/api";

const shipmentStatusSchema = z.enum(shipmentStatuses);

export const shipmentFiltersSchema = z
  .object({
    code: z.string().optional(),
    fromDate: z.string().optional(),
    receiverPhone: z.string().optional(),
    status: z.union([z.literal(""), shipmentStatusSchema]).optional(),
    toDate: z.string().optional(),
  })
  .refine(
    (value) => !value.fromDate || !value.toDate || value.fromDate <= value.toDate,
    {
      message: "Ngày kết thúc phải lớn hơn hoặc bằng ngày bắt đầu.",
      path: ["toDate"],
    },
  );

export type ShipmentFiltersSchema = z.infer<typeof shipmentFiltersSchema>;

export const shipmentStatusTransitionSchema = z.object({
  hubCode: z.string().max(64, "Mã hub quá dài.").optional(),
  location: z.string().max(128, "Vị trí quá dài.").optional(),
  note: z.string().max(500, "Ghi chú tối đa 500 ký tự.").optional(),
  occurredAt: z.string().optional(),
  toStatus: shipmentStatusSchema,
});

export type ShipmentStatusTransitionSchema = z.infer<typeof shipmentStatusTransitionSchema>;