import { ArrowRightLeft, ShieldCheck } from "lucide-react";

import { LoginForm } from "@/components/forms/login-form";
import { Badge } from "@/components/ui/badge";

export default function LoginPage() {
  return (
    <main className="relative flex min-h-screen items-center justify-center overflow-hidden px-4 py-12">
      <div className="absolute inset-0 bg-[radial-gradient(circle_at_top_left,rgba(6,182,212,0.16),transparent_25%),radial-gradient(circle_at_bottom_right,rgba(16,185,129,0.18),transparent_24%)]" />
      <div className="relative grid w-full max-w-6xl gap-10 lg:grid-cols-[1.1fr_0.9fr] lg:items-center">
        <section className="space-y-6">
          <Badge variant="secondary" className="w-fit">Next.js App Router + Tailwind + shadcn/ui</Badge>
          <h1 className="max-w-2xl text-4xl font-semibold leading-tight sm:text-5xl">
            Van hanh giao nhan tren mot control plane toi gian, ro rang va san sang mo rong.
          </h1>
          <p className="max-w-xl text-base text-muted-foreground sm:text-lg">
            Frontend nay duoc scaffold rieng tai `/web`, bam sat product requirements va API contract hien tai cua repo logistics.
          </p>
          <div className="grid gap-4 sm:grid-cols-2">
            <div className="surface-panel p-5">
              <ShieldCheck className="mb-3 h-5 w-5 text-primary" />
              <h2 className="font-medium">Auth-first shell</h2>
              <p className="mt-2 text-sm text-muted-foreground">Login that su goi endpoint auth. Khi backend chua chay, app fallback vao placeholder session co kiem soat.</p>
            </div>
            <div className="surface-panel p-5">
              <ArrowRightLeft className="mb-3 h-5 w-5 text-primary" />
              <h2 className="font-medium">Search and tracking aware</h2>
              <p className="mt-2 text-sm text-muted-foreground">Dashboard, shipments, tracking va search da duoc map theo contract docs va co state loading, empty, error day du.</p>
            </div>
          </div>
        </section>
        <div className="flex justify-center lg:justify-end">
          <LoginForm />
        </div>
      </div>
    </main>
  );
}