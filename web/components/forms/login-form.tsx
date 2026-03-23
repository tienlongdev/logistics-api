"use client";

import { zodResolver } from "@hookform/resolvers/zod";
import { useMutation } from "@tanstack/react-query";
import { ArrowRight, ShieldCheck } from "lucide-react";
import { useRouter } from "next/navigation";
import { useForm } from "react-hook-form";
import { toast } from "sonner";

import { Button } from "@/components/ui/button";
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "@/components/ui/card";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { login } from "@/lib/api/client";
import { useAuthStore } from "@/lib/store/auth-store";
import { loginSchema, type LoginSchema } from "@/lib/validation/auth";

export function LoginForm() {
  const router = useRouter();
  const setSession = useAuthStore((state) => state.setSession);
  const form = useForm<LoginSchema>({
    resolver: zodResolver(loginSchema),
    defaultValues: {
      email: "merchant@demo.com",
      password: "P@ssw0rd!",
    },
  });

  const mutation = useMutation({
    mutationFn: login,
    onSuccess: (response, values) => {
      setSession({
        accessToken: response.accessToken,
        refreshToken: response.refreshToken,
        expiresIn: response.expiresIn,
        email: values.email,
        mode: "api",
      });
      toast.success("Đăng nhập thành công!");
      router.push("/shipments");
    },
    onError: () => {
      toast.error("Email hoặc mật khẩu không đúng. Vui lòng thử lại.");
    },
  });

  return (
    <Card className="w-full max-w-md border-primary/10 bg-card/95">
      <CardHeader>
        <div className="mb-4 flex h-12 w-12 items-center justify-center rounded-2xl bg-primary/10 text-primary">
          <ShieldCheck className="h-6 w-6" />
        </div>
        <CardTitle>Đăng nhập</CardTitle>
        <CardDescription>
          Nhập email và mật khẩu để truy cập hệ thống quản lý vận chuyển.
        </CardDescription>
      </CardHeader>
      <CardContent>
        <form className="space-y-4" onSubmit={form.handleSubmit((values) => mutation.mutate(values))}>
          <div className="space-y-2">
            <Label htmlFor="email">Địa chỉ email</Label>
            <Input id="email" type="email" autoComplete="email" aria-invalid={Boolean(form.formState.errors.email)} {...form.register("email")} />
            {form.formState.errors.email ? (
              <p className="text-sm text-destructive">{form.formState.errors.email.message}</p>
            ) : null}
          </div>
          <div className="space-y-2">
            <Label htmlFor="password">Mật khẩu</Label>
            <Input
              id="password"
              type="password"
              autoComplete="current-password"
              aria-invalid={Boolean(form.formState.errors.password)}
              {...form.register("password")}
            />
            {form.formState.errors.password ? (
              <p className="text-sm text-destructive">{form.formState.errors.password.message}</p>
            ) : null}
          </div>
          <Button type="submit" className="w-full" disabled={mutation.isPending}>
            {mutation.isPending ? "Đang xử lý..." : "Đăng nhập"}
            {!mutation.isPending && <ArrowRight className="h-4 w-4" />}
          </Button>
        </form>
      </CardContent>
    </Card>
  );
}