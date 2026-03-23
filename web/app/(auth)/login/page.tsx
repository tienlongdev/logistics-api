import { ArrowRightLeft, ShieldCheck } from "lucide-react";

import { LoginForm } from "@/components/forms/login-form";
import { Badge } from "@/components/ui/badge";

export default function LoginPage() {
  return (
    <main className="relative flex min-h-screen items-center justify-center overflow-hidden px-4 py-12">
      <div className="absolute inset-0 bg-[radial-gradient(circle_at_top_left,rgba(6,182,212,0.16),transparent_25%),radial-gradient(circle_at_bottom_right,rgba(16,185,129,0.18),transparent_24%)]" />
      <div className="relative grid w-full max-w-6xl gap-10 lg:grid-cols-[1.1fr_0.9fr] lg:items-center">
        <section className="space-y-6">
          <Badge variant="secondary" className="w-fit">Next.js + TanStack Query + Zustand + shadcn/ui</Badge>
          <h1 className="max-w-2xl text-4xl font-semibold leading-tight sm:text-5xl">
            Dieu phoi shipment bang UI bam sat contract backend, khong dung endpoint tu dat ra.
          </h1>
          <p className="max-w-xl text-base text-muted-foreground sm:text-lg">
            Login se cap access token trong memory va su dung refresh flow that khi gap `401`, dung voi contract auth hien tai cua backend.
          </p>
          <div className="grid gap-4 sm:grid-cols-2">
            <div className="surface-panel p-5">
              <ShieldCheck className="mb-3 h-5 w-5 text-primary" />
              <h2 className="font-medium">Contract-first auth</h2>
              <p className="mt-2 text-sm text-muted-foreground">`POST /auth/login`, `POST /auth/refresh`, `POST /auth/logout` da duoc noi truc tiep vao typed client.</p>
            </div>
            <div className="surface-panel p-5">
              <ArrowRightLeft className="mb-3 h-5 w-5 text-primary" />
              <h2 className="font-medium">Graceful backend gaps</h2>
              <p className="mt-2 text-sm text-muted-foreground">Cac man hinh shipment detail va cancel se render UI + TODO ro rang neu backend chua co endpoint tuong ung.</p>
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