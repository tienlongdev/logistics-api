import { z } from "zod";

const shipmentStatusSchema = z.enum([
  "Created",
  "AwaitingPickup",
  "PickedUp",
  "InboundAtOriginHub",
  "InTransit",
  "InboundAtDestinationHub",
  "OutForDelivery",
  "DeliveryAttemptFailed",
  "Delivered",
  "Cancelled",
  "Returned",
  "ReturnInTransit",
  "ReturnedToSender",
]);

export const searchFiltersSchema = z.object({
  trackingCode: z.string().optional(),
  shipmentCode: z.string().optional(),
  merchantCode: z.string().optional(),
  receiverPhone: z.string().optional(),
  status: z.union([z.literal(""), shipmentStatusSchema]).optional(),
  fromDate: z.string().optional(),
  toDate: z.string().optional(),
  sort: z.string().default("updatedAt:desc"),
});

export type SearchFiltersSchema = z.infer<typeof searchFiltersSchema>;

export const trackingLookupSchema = z.object({
  trackingCode: z.string().min(5, "Nhap tracking code."),
});

export type TrackingLookupSchema = z.infer<typeof trackingLookupSchema>;

export const settingsSchema = z.object({
  displayName: z.string().min(2, "Nhap ten hien thi."),
  emailDigest: z.boolean().default(true),
  compactDensity: z.boolean().default(false),
});

export type SettingsSchema = z.infer<typeof settingsSchema>;