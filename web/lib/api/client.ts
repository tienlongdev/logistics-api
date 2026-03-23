import { buildApiUrl } from "@/lib/api/base";
import { apiEndpoints } from "@/lib/api/endpoints";
import { shipmentStatusValueMap } from "@/lib/constants/status";
import { useAuthStore } from "@/lib/store/auth-store";
import {
    type DashboardSnapshot,
    type LoginRequest,
    type LoginResponse,
    type ProblemDetails,
    type SearchShipmentFilters,
    type SearchShipmentsResponse,
    type TrackingSummary,
    type TrackingTimelineResponse,
    type TransitionShipmentStatusRequest,
    type TransitionShipmentStatusResponse,
} from "@/lib/types/api";
import { toast } from "sonner";

let refreshPromise: Promise<string | null> | null = null;

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

type QueryValue = boolean | number | string | undefined;

interface RequestOptions extends Omit<RequestInit, "body"> {
  auth?: boolean;
  body?: BodyInit | object;
  query?: Record<string, QueryValue>;
  retryOnUnauthorized?: boolean;
  suppressToast?: boolean;
  token?: string | null;
}

function createCorrelationId() {
  if (typeof crypto !== "undefined" && typeof crypto.randomUUID === "function") {
    return crypto.randomUUID();
  }

  return `web-${Date.now()}-${Math.random().toString(16).slice(2)}`;
}

function buildMessage(problem?: ProblemDetails, fallback = "Request failed") {
  const validationErrors = problem?.errors
    ? Object.values(problem.errors)
        .flat()
        .filter(Boolean)
        .join(" ")
    : "";

  return validationErrors || problem?.detail || problem?.title || fallback;
}

function normalizeError(error: unknown) {
  if (error instanceof ApiError) {
    return error;
  }

  if (error instanceof TypeError) {
    return new ApiError(
      "Backend not available. Start the API stack and verify the configured API base URLs.",
      503,
    );
  }

  return new ApiError("Unexpected request failure.", 500);
}

async function parseResponse<T>(response: Response) {
  if (response.status === 204) {
    return undefined as T;
  }

  return (await response.json()) as T;
}

async function refreshAccessToken() {
  if (refreshPromise) {
    return refreshPromise;
  }

  refreshPromise = (async () => {
    const { clearSession, refreshToken, updateTokens } = useAuthStore.getState();

    if (!refreshToken) {
      return null;
    }

    try {
      const response = await request<LoginResponse>(apiEndpoints.refresh, {
        auth: false,
        body: { refreshToken },
        method: "POST",
        retryOnUnauthorized: false,
        suppressToast: true,
      });

      updateTokens(response);
      return response.accessToken;
    } catch {
      clearSession();
      return null;
    } finally {
      refreshPromise = null;
    }
  })();

  return refreshPromise;
}

async function request<T>(path: string, options: RequestOptions = {}): Promise<T> {
  const {
    auth = true,
    body,
    headers,
    query,
    retryOnUnauthorized = true,
    suppressToast = false,
    token,
    ...init
  } = options;

  try {
    const accessToken = token !== undefined ? token : auth ? useAuthStore.getState().accessToken : null;
    const response = await fetch(buildApiUrl(path, query), {
      ...init,
      body:
        body === undefined || body instanceof FormData || typeof body === "string"
          ? body
          : JSON.stringify(body),
      cache: "no-store",
      headers: {
        ...(body === undefined || body instanceof FormData ? {} : { "Content-Type": "application/json" }),
        ...(accessToken ? { Authorization: `Bearer ${accessToken}` } : {}),
        "X-Correlation-Id": createCorrelationId(),
        ...(headers ?? {}),
      },
    });

    if (response.status === 401 && auth && retryOnUnauthorized && !path.startsWith("auth/")) {
      const refreshedAccessToken = await refreshAccessToken();

      if (refreshedAccessToken) {
        return request<T>(path, {
          ...options,
          retryOnUnauthorized: false,
          token: refreshedAccessToken,
        });
      }
    }

    if (!response.ok) {
      const maybeProblem = (await response.json().catch(() => undefined)) as ProblemDetails | undefined;
      throw new ApiError(buildMessage(maybeProblem), response.status, maybeProblem);
    }

    return parseResponse<T>(response);
  } catch (error) {
    const normalizedError = normalizeError(error);

    if (!suppressToast) {
      toast.error(normalizedError.message);
    }

    throw normalizedError;
  }
}

function toStartOfDay(value?: string) {
  return value ? `${value}T00:00:00.000Z` : undefined;
}

function toEndOfDay(value?: string) {
  return value ? `${value}T23:59:59.999Z` : undefined;
}

export async function login(payload: LoginRequest) {
  return request<LoginResponse>(apiEndpoints.login, {
    auth: false,
    body: payload,
    method: "POST",
    retryOnUnauthorized: false,
  });
}

export async function logout() {
  const { clearSession, refreshToken } = useAuthStore.getState();

  try {
    if (refreshToken) {
      await request<void>(apiEndpoints.logout, {
        auth: false,
        body: { refreshToken },
        method: "POST",
        retryOnUnauthorized: false,
        suppressToast: true,
      });
    }
  } finally {
    clearSession();
  }
}

export async function restoreSession() {
  const restoredToken = await refreshAccessToken();
  return Boolean(restoredToken);
}

export function searchShipments(filters: SearchShipmentFilters, token?: string) {
  return request<SearchShipmentsResponse>(apiEndpoints.searchShipments, {
    query: {
      TrackingCode: filters.trackingCode,
      ShipmentCode: filters.shipmentCode,
      MerchantCode: filters.merchantCode,
      ReceiverPhone: filters.receiverPhone,
      Status: filters.status,
      FromDate: toStartOfDay(filters.fromDate),
      ToDate: toEndOfDay(filters.toDate),
      Page: filters.page ?? 1,
      PageSize: filters.pageSize ?? 10,
      Sort: filters.sort ?? "updatedAt:desc",
    },
    token,
  });
}

export function getTrackingSummary(trackingCode: string) {
  return request<TrackingSummary>(apiEndpoints.trackingSummary(trackingCode), {
    auth: false,
  });
}

export function getTrackingTimeline(trackingCode: string) {
  return request<TrackingTimelineResponse>(apiEndpoints.trackingTimeline(trackingCode), {
    auth: false,
  });
}

export function transitionShipmentStatus(
  shipmentId: string,
  payload: TransitionShipmentStatusRequest,
  token?: string,
) {
  return request<TransitionShipmentStatusResponse>(apiEndpoints.shipmentStatusTransitions(shipmentId), {
    body: {
      toStatus: shipmentStatusValueMap[payload.toStatus],
      hubId: payload.hubId ?? null,
      hubCode: payload.hubCode ?? null,
      location: payload.location ?? null,
      note: payload.note ?? null,
      occurredAt: payload.occurredAt ?? null,
    },
    method: "POST",
    token,
  });
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