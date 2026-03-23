import { PackageCheck, Route, Search } from "lucide-react";

import { LoginForm } from "@/components/forms/login-form";

export default function LoginPage() {
  return (
    <main className="relative flex min-h-screen items-center justify-center overflow-hidden px-4 py-12">
      <div className="absolute inset-0 bg-[radial-gradient(circle_at_top_left,rgba(8,145,178,0.18),transparent_24%),radial-gradient(circle_at_bottom_right,rgba(15,23,42,0.1),transparent_24%)]" />
      <div className="relative grid w-full max-w-6xl gap-10 lg:grid-cols-[1.1fr_0.9fr] lg:items-center">
        <section className="space-y-6">
          <p className="section-kicker">Nền tảng vận chuyển</p>
          <h1 className="max-w-2xl text-4xl font-semibold leading-tight sm:text-5xl">
            Quản lý đơn hàng, theo dõi vận chuyển — mọi lúc, mọi nơi.
          </h1>
          <p className="max-w-xl text-base text-muted-foreground sm:text-lg">
            Đăng nhập để xem toàn bộ đơn hàng, trạng thái vận chuyển và lịch sử giao hàng của bạn.
          </p>
          <div className="grid gap-4 sm:grid-cols-3">
            <div className="surface-panel p-5">
              <PackageCheck className="mb-3 h-5 w-5 text-primary" />
              <h2 className="font-medium">Theo dõi đơn hàng</h2>
              <p className="mt-2 text-sm text-muted-foreground">Xem trạng thái toàn bộ đơn hàng theo thời gian thực.</p>
            </div>
            <div className="surface-panel p-5">
              <Route className="mb-3 h-5 w-5 text-primary" />
              <h2 className="font-medium">Tra cứu vận đơn</h2>
              <p className="mt-2 text-sm text-muted-foreground">Xem hành trình giao hàng chi tiết theo mã vận đơn.</p>
            </div>
            <div className="surface-panel p-5">
              <Search className="mb-3 h-5 w-5 text-primary" />
              <h2 className="font-medium">Tìm kiếm nhanh</h2>
              <p className="mt-2 text-sm text-muted-foreground">Lọc đơn hàng theo người nhận, trạng thái và ngày giao.</p>
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