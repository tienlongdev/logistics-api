import { z } from "zod";

export const loginSchema = z.object({
  email: z.string().email("Nhap email hop le."),
  password: z.string().min(8, "Mat khau toi thieu 8 ky tu."),
});

export type LoginSchema = z.infer<typeof loginSchema>;