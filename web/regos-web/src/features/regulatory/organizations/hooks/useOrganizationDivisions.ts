import { useQuery } from "@tanstack/react-query";

import { listOrganizationDivisions } from "../api/listOrganizationDivisions";

export function useOrganizationDivisions(organizationId: string) {
  return useQuery({
    queryKey: ["organizations", organizationId, "divisions"],
    queryFn: () => listOrganizationDivisions(organizationId),
  });
}
