const DEFAULT_PUBLIC_API_BASE_URL = "http://localhost:5080";

function normalizeApiBaseUrl(value: string) {
  const trimmedValue = value.replace(/\/+$/, "");

  return trimmedValue.endsWith("/api/v1") ? trimmedValue : `${trimmedValue}/api/v1`;
}

export function getPublicApiBaseUrl() {
  return normalizeApiBaseUrl(process.env.NEXT_PUBLIC_API_BASE_URL ?? DEFAULT_PUBLIC_API_BASE_URL);
}

export function getInternalApiBaseUrl() {
  return normalizeApiBaseUrl(
    process.env.API_INTERNAL_BASE_URL ?? process.env.NEXT_PUBLIC_API_BASE_URL ?? DEFAULT_PUBLIC_API_BASE_URL,
  );
}

export function getApiBaseUrl() {
  return typeof window === "undefined" ? getInternalApiBaseUrl() : getPublicApiBaseUrl();
}