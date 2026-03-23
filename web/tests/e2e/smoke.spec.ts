import { expect, test } from "@playwright/test";

test("login page renders polished shell", async ({ page }) => {
  await page.goto("/login");

  await expect(page.getByRole("heading", { name: /Dang nhap vao Logistics Web/i })).toBeVisible();
  await expect(page.getByRole("button", { name: /Dang nhap/i })).toBeVisible();
});

test("tracking page supports public lookup entry", async ({ page }) => {
  await page.goto("/tracking");

  await expect(page.getByRole("heading", { name: /Public timeline lookup/i })).toBeVisible();
  await page.keyboard.press("/");
  await expect(page.getByLabel(/Tracking code/i)).toBeFocused();
});

test("not-found page offers recovery links", async ({ page }) => {
  await page.goto("/missing-route");

  await expect(page.getByRole("heading", { name: /Khong tim thay trang/i })).toBeVisible();
  await expect(page.getByRole("link", { name: /Ve dashboard/i })).toBeVisible();
});