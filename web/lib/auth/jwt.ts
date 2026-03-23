import type { ApiRole } from "@/lib/types/api";

const roleClaimKeys = [
  "http://schemas.microsoft.com/ws/2008/06/identity/claims/role",
  "role",
  "roles",
] as const;

interface JwtPayload {
  email?: string;
  sub?: string;
  [key: string]: unknown;
}

export interface DecodedAccessToken {
  email?: string;
  roles: ApiRole[];
  subject?: string;
}

function isApiRole(value: unknown): value is ApiRole {
  return value === "Admin" || value === "Operator" || value === "HubStaff" || value === "Merchant";
}

function decodeBase64Url(value: string) {
  const normalized = value.replace(/-/g, "+").replace(/_/g, "/");
  const padded = normalized.padEnd(Math.ceil(normalized.length / 4) * 4, "=");

  if (typeof atob === "function") {
    return decodeURIComponent(
      Array.from(atob(padded), (character) =>
        `%${character.charCodeAt(0).toString(16).padStart(2, "0")}`,
      ).join(""),
    );
  }

  return Buffer.from(padded, "base64").toString("utf-8");
}

function readRoles(payload: JwtPayload) {
  const roles = roleClaimKeys.flatMap((key) => {
    const value = payload[key];

    if (Array.isArray(value)) {
      return value.filter(isApiRole);
    }

    return isApiRole(value) ? [value] : [];
  });

  return Array.from(new Set(roles));
}

export function decodeAccessToken(token: string): DecodedAccessToken {
  try {
    const [, payloadSegment] = token.split(".");

    if (!payloadSegment) {
      return { roles: [] };
    }

    const payload = JSON.parse(decodeBase64Url(payloadSegment)) as JwtPayload;

    return {
      email: payload.email,
      roles: readRoles(payload),
      subject: payload.sub,
    };
  } catch {
    return { roles: [] };
  }
}