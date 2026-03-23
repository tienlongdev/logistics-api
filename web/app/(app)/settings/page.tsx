"use client";

import { zodResolver } from "@hookform/resolvers/zod";
import { useTheme } from "next-themes";
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
      displayName: "Operations Manager",
      emailDigest: true,
      compactDensity: false,
    },
  });

  return (
    <div className="grid gap-6">
      <div className="page-header">
        <div>
          <p className="text-sm font-medium uppercase tracking-[0.2em] text-primary">Settings</p>
          <h2 className="text-3xl font-semibold">Workspace preferences</h2>
        </div>
        <p className="max-w-xl text-sm text-muted-foreground">Trang nay giu vai tro placeholder cho profile va UI preferences. Tat ca validate bang zod truoc khi luu local.</p>
      </div>

      <Card>
        <CardHeader>
          <CardTitle>Profile and notifications</CardTitle>
          <CardDescription>Khong goi backend. Save local de hoan thien luong settings va toast.</CardDescription>
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
              <Label htmlFor="display-name">Display name</Label>
              <Input id="display-name" {...form.register("displayName")} />
              {form.formState.errors.displayName ? <p className="text-sm text-destructive">{form.formState.errors.displayName.message}</p> : null}
            </div>

            <div className="rounded-2xl border border-border/70 bg-background/70 p-4">
              <div className="flex items-center justify-between gap-4">
                <div>
                  <p className="font-medium">Email digest</p>
                  <p className="text-sm text-muted-foreground">Nhan tong hop theo ngay cho shipment progress va webhook failures.</p>
                </div>
                <input type="checkbox" className="h-4 w-4" aria-label="Toggle email digest" {...form.register("emailDigest")} />
              </div>
            </div>

            <div className="rounded-2xl border border-border/70 bg-background/70 p-4">
              <div className="flex items-center justify-between gap-4">
                <div>
                  <p className="font-medium">Compact density</p>
                  <p className="text-sm text-muted-foreground">Dua table va cards ve mat do hien thi dam hon.</p>
                </div>
                <input type="checkbox" className="h-4 w-4" aria-label="Toggle compact density" {...form.register("compactDensity")} />
              </div>
            </div>

            <div className="rounded-2xl border border-border/70 bg-background/70 p-4">
              <Label htmlFor="theme-mode">Theme mode</Label>
              <select
                id="theme-mode"
                value={theme}
                onChange={(event) => setTheme(event.target.value)}
                className="mt-2 flex h-10 w-full max-w-xs rounded-md border border-input bg-background px-3 py-2 text-sm"
              >
                <option value="light">Light</option>
                <option value="dark">Dark</option>
                <option value="system">System</option>
              </select>
            </div>

            <div>
              <Button type="submit">Luu thay doi</Button>
            </div>
          </form>
        </CardContent>
      </Card>
    </div>
  );
}