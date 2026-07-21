import { apiFetch, buildUrl } from "@/shared/api/apiClient";

import type { PagedResult } from "../types/PagedResult";
import type { UserListItem } from "../types/UserListItem";

export interface ListUsersParams {
  search?: string;
  status?: string;
  page?: number;
  pageSize?: number;
}

export async function listUsers(
  params: ListUsersParams,
): Promise<PagedResult<UserListItem>> {
  const query = new URLSearchParams();

  if (params.search) query.set("search", params.search);
  if (params.status) query.set("status", params.status);
  if (params.page) query.set("page", String(params.page));
  if (params.pageSize) query.set("pageSize", String(params.pageSize));

  const suffix = query.toString() ? `?${query}` : "";

  const response = await apiFetch(buildUrl(`/api/platform/users${suffix}`));

  if (!response.ok) {
    throw new Error("Unable to load users.");
  }

  return response.json();
}
