import { useQuery } from "@tanstack/react-query";

import { listOrganizationContacts } from "../api/listOrganizationContacts";

export function useOrganizationContacts(organizationId: string) {
  return useQuery({
    queryKey: ["organizations", organizationId, "contacts"],
    queryFn: () => listOrganizationContacts(organizationId),
  });
}
