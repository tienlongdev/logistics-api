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
      message: "Ngay ket thuc phai lon hon hoac bang ngay bat dau.",
      path: ["toDate"],
    },
  );

export type ShipmentFiltersSchema = z.infer<typeof shipmentFiltersSchema>;

export const shipmentStatusTransitionSchema = z.object({
  hubCode: z.string().max(64, "Hub code qua dai.").optional(),
  location: z.string().max(128, "Location qua dai.").optional(),
  note: z.string().max(500, "Ghi chu toi da 500 ky tu.").optional(),
  occurredAt: z.string().optional(),
  toStatus: shipmentStatusSchema,
});

export type ShipmentStatusTransitionSchema = z.infer<typeof shipmentStatusTransitionSchema>;