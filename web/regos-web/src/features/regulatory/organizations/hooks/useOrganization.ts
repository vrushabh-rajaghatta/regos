import { useQuery } from "@tanstack/react-query";

import {
  getOrganization,
  OrganizationNotFoundError,
} from "../api/getOrganization";

export function useOrganization(organizationId: string) {
  return useQuery({
    queryKey: ["organizations", organizationId],
    queryFn: () => getOrganization(organizationId),

    // A missing organization is an answer, not a failure worth retrying.
    retry: (failureCount, error) =>
      !(error instanceof OrganizationNotFoundError) && failureCount < 3,
  });
}
