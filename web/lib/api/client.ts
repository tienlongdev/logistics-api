import { apiEndpoints } from "@/lib/api/endpoints";
import {
    mockShipmentsResponse,
    mockTrackingSummary,
    mockTrackingTimeline,
} from "@/lib/mock/shipments";
import {
    type DashboardSnapshot,
    type LoginRequest,
    type LoginResponse,
    type ProblemDetails,
    type SearchShipmentFilters,
    type SearchShipmentsResponse,
    type TrackingSummary,
    type TrackingTimelineResponse,
} from "@/lib/types/api";

const API_BASE_URL = process.env.NEXT_PUBLIC_API_BASE_URL ?? "http://localhost:8080/api/v1";
const ENABLE_MOCK_FALLBACK = process.env.NEXT_PUBLIC_ENABLE_MOCK_FALLBACK !== "false";

export class ApiError extends Error {
  status: number;
  problem?: ProblemDetails;

  constructor(message: string, status: number, problem?: ProblemDetails) {
    super(message);
    this.name = "ApiError";
    this.status = status;
    this.problem = problem;
  }
}

function buildUrl(path: string, query?: Record<string, string | number | undefined>) {
  const url = new URL(path, API_BASE_URL.endsWith("/") ? API_BASE_URL : `${API_BASE_URL}/`);

  if (query) {
    Object.entries(query).forEach(([key, value]) => {
      if (value !== undefined && value !== "") {
        url.searchParams.set(key, String(value));
      }
    });
  }

  return url.toString();
}

async function request<T>(path: string, init?: RequestInit, token?: string): Promise<T> {
  const response = await fetch(path.startsWith("http") ? path : buildUrl(path), {
    ...init,
    headers: {
      "Content-Type": "application/json",
      ...(token ? { Authorization: `Bearer ${token}` } : {}),
      ...(init?.headers ?? {}),
    },
    cache: "no-store",
  });

  if (!response.ok) {
    const maybeProblem = (await response.json().catch(() => undefined)) as ProblemDetails | undefined;
    throw new ApiError(maybeProblem?.detail ?? maybeProblem?.title ?? "Request failed", response.status, maybeProblem);
  }

  if (response.status === 204) {
    return undefined as T;
  }

  return (await response.json()) as T;
}

function withMockFallback<T>(fetcher: () => Promise<T>, fallback: T) {
  return fetcher().catch((error) => {
    if (ENABLE_MOCK_FALLBACK && error instanceof TypeError) {
      return fallback;
    }

    throw error;
  });
}

export async function login(payload: LoginRequest) {
  try {
    return await request<LoginResponse>(apiEndpoints.login, {
      method: "POST",
      body: JSON.stringify(payload),
    });
  } catch (error) {
    if (ENABLE_MOCK_FALLBACK && error instanceof TypeError) {
      return {
        accessToken: "demo-access-token",
        refreshToken: "demo-refresh-token",
        expiresIn: 3600,
      } satisfies LoginResponse;
    }

    throw error;
  }
}

export function searchShipments(filters: SearchShipmentFilters, token?: string) {
  if (token === "demo-access-token") {
    return Promise.resolve(mockShipmentsResponse);
  }

  const query = Object.fromEntries(
    Object.entries({
      trackingCode: filters.trackingCode,
      shipmentCode: filters.shipmentCode,
      merchantCode: filters.merchantCode,
      receiverPhone: filters.receiverPhone,
      status: filters.status,
      fromDate: filters.fromDate,
      toDate: filters.toDate,
      page: filters.page ?? 1,
      pageSize: filters.pageSize ?? 10,
      sort: filters.sort ?? "updatedAt:desc",
    }).filter(([, value]) => value !== undefined && value !== ""),
  );

  return withMockFallback(
    () => request<SearchShipmentsResponse>(buildUrl(apiEndpoints.searchShipments, query), undefined, token),
    mockShipmentsResponse,
  );
}

export function getTrackingSummary(trackingCode: string) {
  return withMockFallback(
    () => request<TrackingSummary>(apiEndpoints.trackingSummary(trackingCode)),
    mockTrackingSummary,
  );
}

export function getTrackingTimeline(trackingCode: string) {
  return withMockFallback(
    () => request<TrackingTimelineResponse>(apiEndpoints.trackingTimeline(trackingCode)),
    mockTrackingTimeline,
  );
}

export async function getDashboardSnapshot(token?: string): Promise<DashboardSnapshot> {
  const response = await searchShipments({ page: 1, pageSize: 6, sort: "updatedAt:desc" }, token);

  return {
    items: response.items,
    total: response.total,
    delivered: response.items.filter((item) => item.status === "Delivered").length,
    inTransit: response.items.filter((item) => item.status === "InTransit").length,
    cancelled: response.items.filter((item) => item.status === "Cancelled").length,
  };
}