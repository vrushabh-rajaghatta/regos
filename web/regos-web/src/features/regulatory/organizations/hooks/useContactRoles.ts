import { useQuery } from "@tanstack/react-query";

import { listContactRoles } from "../api/listContactRoles";

/**
 * Shared-plus-extensible reference data: the platform's roles plus this
 * tenant's own. Cached for the session rather than the hour used for
 * identifier schemes — a tenant can add a role, and would expect to see it.
 */
export function useContactRoles() {
  return useQuery({
    queryKey: ["reference-data", "contact-roles"],
    queryFn: listContactRoles,
    staleTime: 5 * 60 * 1000,
  });
}
