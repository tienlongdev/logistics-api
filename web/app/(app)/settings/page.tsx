"use client";

import { zodResolver } from "@hookform/resolvers/zod";
import { BellRing, LayoutGrid, UserRound } from "lucide-react";
import { useTheme } from "next-themes";
import { useEffect } from "react";
import { useForm } from "react-hook-form";
import { toast } from "sonner";

import { Button } from "@/components/ui/button";
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "@/components/ui/card";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { settingsSchema, type SettingsSchema } from "@/lib/validation/search";

export default function SettingsPage() {
  const { theme, setTheme } = useTheme();
  const form = useForm<SettingsSchema>({
    resolver: zodResolver(settingsSchema),
    defaultValues: {
      displayName: "Quản lý vận hành",
      emailDigest: true,
      compactDensity: false,
    },
  });

  useEffect(() => {
    const saved = localStorage.getItem("logistics-web-settings");

    if (!saved) {
      return;
    }

    try {
      form.reset(JSON.parse(saved) as SettingsSchema);
    } catch {
      localStorage.removeItem("logistics-web-settings");
    }
  }, [form]);

  return (
    <div className="grid gap-6">
      <div className="page-header">
        <div className="space-y-3">
          <p className="section-kicker">Cài đặt</p>
          <h2 className="text-3xl font-semibold sm:text-4xl">Tuỳ chỉnh cài đặt</h2>
          <p className="page-copy">Tuỳ chỉnh giao diện và thông báo theo sở thích của bạn.</p>
        </div>
      </div>

      <Card>
        <CardHeader>
          <CardTitle>Thông tin cá nhân &amp; thông báo</CardTitle>
          <CardDescription>Quản lý tên hiển thị, tùy chỉnh thông báo và giao diện.</CardDescription>
        </CardHeader>
        <CardContent>
          <form
            className="grid gap-4 md:max-w-2xl"
            onSubmit={form.handleSubmit((values) => {
              localStorage.setItem("logistics-web-settings", JSON.stringify(values));
              toast.success("Da luu settings local");
            })}
          >
            <div className="space-y-2">
              <Label htmlFor="display-name">Tên hiển thị</Label>
              <Input id="display-name" {...form.register("displayName")} />
              {form.formState.errors.displayName ? <p className="text-sm text-destructive">{form.formState.errors.displayName.message}</p> : null}
            </div>

            <div className="rounded-[1.2rem] border border-border/70 bg-background/70 p-4">
              <div className="flex items-center justify-between gap-4">
                <div>
                  <p className="flex items-center gap-2 font-medium"><BellRing className="h-4 w-4 text-primary" />Thông báo email</p>
                  <p className="text-sm text-muted-foreground">Nhận báo cáo tổng hợp hằng ngày về tiến độ đơn hàng.</p>
                </div>
                <input type="checkbox" className="h-4 w-4" aria-label="Toggle email digest" {...form.register("emailDigest")} />
              </div>
            </div>

            <div className="rounded-[1.2rem] border border-border/70 bg-background/70 p-4">
              <div className="flex items-center justify-between gap-4">
                <div>
                  <p className="flex items-center gap-2 font-medium"><LayoutGrid className="h-4 w-4 text-primary" />Giao diện gọn</p>
                  <p className="text-sm text-muted-foreground">Hiển thị bảng và danh sách ở mật độ cao hơn.</p>
                </div>
                <input type="checkbox" className="h-4 w-4" aria-label="Toggle compact density" {...form.register("compactDensity")} />
              </div>
            </div>

            <div className="rounded-[1.2rem] border border-border/70 bg-background/70 p-4">
              <Label htmlFor="theme-mode">Giao diện</Label>
              <select
                id="theme-mode"
                value={theme}
                onChange={(event) => setTheme(event.target.value)}
                className="mt-2 flex h-10 w-full max-w-xs rounded-md border border-input bg-background px-3 py-2 text-sm"
              >
                <option value="light">Sáng</option>
                <option value="dark">Tối</option>
                <option value="system">Theo hệ thống</option>
              </select>
            </div>

            <div>
              <Button type="submit">
                <UserRound className="h-4 w-4" />
                Lưu thay đổi
              </Button>
            </div>
          </form>
        </CardContent>
      </Card>
    </div>
  );
}