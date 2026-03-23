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
    label: "Dashboard",
    description: "Tong quan luong don va muc suc khoe van hanh.",
    icon: Gauge,
  },
  {
    href: "/shipments",
    label: "Shipments",
    description: "Quan sat danh sach don va chi phi COD.",
    icon: PackageSearch,
  },
  {
    href: "/tracking",
    label: "Tracking",
    description: "Tra cuu hanh trinh theo tracking code.",
    icon: Route,
  },
  {
    href: "/search",
    label: "Search",
    description: "Tim kiem nang cao tren Elasticsearch.",
    icon: Search,
  },
  {
    href: "/settings",
    label: "Settings",
    description: "Cau hinh giao dien, canh bao va profile.",
    icon: Settings,
  },
] as const;

export const routeTitles = Object.fromEntries(appNavigation.map((item) => [item.href, item.label]));