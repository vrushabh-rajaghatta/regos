import { useQuery } from "@tanstack/react-query";

import { listOrganizationSites } from "../api/listOrganizationSites";

export function useOrganizationSites(organizationId: string) {
  return useQuery({
    queryKey: ["organizations", organizationId, "sites"],
    queryFn: () => listOrganizationSites(organizationId),
  });
}
