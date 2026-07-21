import { useQuery } from "@tanstack/react-query";

import { listOrganizations } from "../api/listOrganizations";

/**
 * Shares the "organizations" query key with the regulatory master-data hook of
 * the same name — both read the same endpoint, so creating an organization
 * refreshes the applicant dropdown too.
 */
export function useOrganizationDirectory() {
  return useQuery({
    queryKey: ["organizations"],
    queryFn: listOrganizations,
  });
}
