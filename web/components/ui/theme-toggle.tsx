"use client";

import { MonitorCog, MoonStar, SunMedium } from "lucide-react";
import { useTheme } from "next-themes";

import { Button } from "@/components/ui/button";

export function ThemeToggle() {
  const { theme, setTheme } = useTheme();

  const nextTheme = theme === "light" ? "dark" : theme === "dark" ? "system" : "light";

  return (
    <Button
      type="button"
      variant="outline"
      size="icon"
      aria-label="Switch theme"
      onClick={() => setTheme(nextTheme)}
    >
      {theme === "light" ? <SunMedium className="h-4 w-4" /> : null}
      {theme === "dark" ? <MoonStar className="h-4 w-4" /> : null}
      {theme === "system" ? <MonitorCog className="h-4 w-4" /> : null}
    </Button>
  );
}