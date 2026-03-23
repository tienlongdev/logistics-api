import { getApiBaseUrl } from "@/lib/config";

type QueryValue = boolean | number | string | undefined;

export function buildApiUrl(path: string, query?: Record<string, QueryValue>) {
  const normalizedPath = path.replace(/^\//, "");
  const url = new URL(normalizedPath, `${getApiBaseUrl()}/`);

  if (query) {
    Object.entries(query).forEach(([key, value]) => {
      if (value !== undefined && value !== "") {
        url.searchParams.set(key, String(value));
      }
    });
  }

  return url.toString();
}