import { z } from "zod";

export const loginSchema = z.object({
  email: z.string().email("Vui lòng nhập địa chỉ email hợp lệ."),
  password: z.string().min(8, "Mật khẩu phải có ít nhất 8 ký tự."),
});

export type LoginSchema = z.infer<typeof loginSchema>;