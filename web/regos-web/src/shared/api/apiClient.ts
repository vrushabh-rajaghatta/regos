const API_BASE_URL = import.meta.env.VITE_API_BASE_URL;

const TENANT_ID = import.meta.env.VITE_TENANT_ID;

export const buildUrl = (path: string): string => {
  return `${API_BASE_URL}${path}`;
};

/**
 * Every tenant-scoped API call must identify its tenant; the API rejects the
 * request with a 400 otherwise. Until sign-in exists the value comes from
 * VITE_TENANT_ID, which is a development stand-in for a claim — it decides
 * which organization the UI is looking at, and grants no authority.
 */
export const tenantHeaders = (): Record<string, string> => {
  return { "X-Tenant-Id": TENANT_ID };
};
