import {
    Gauge,
    PackageSearch,
    Route,
    Search,
    Settings,
} from "lucide-react";

export const appNavigation = [
  {
    href: "/dashboard",
    label: "Tổng quan",
    description: "Xem tổng hợp tình trạng đơn hàng đang hoạt động.",
    icon: Gauge,
  },
  {
    href: "/shipments",
    label: "Đơn hàng",
    description: "Tìm kiếm và quản lý danh sách đơn vận chuyển.",
    icon: PackageSearch,
  },
  {
    href: "/tracking",
    label: "Tra cứu",
    description: "Theo dõi hành trình giao hàng theo mã vận đơn.",
    icon: Route,
  },
  {
    href: "/search",
    label: "Tìm kiếm",
    description: "Tìm kiếm nâng cao theo nhiều tiêu chí lọc.",
    icon: Search,
  },
  {
    href: "/settings",
    label: "Cài đặt",
    description: "Tuỳ chỉnh giao diện và thông tin cá nhân.",
    icon: Settings,
  },
] as const;

export const routeTitles = Object.fromEntries(appNavigation.map((item) => [item.href, item.label]));