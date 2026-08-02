import { useQuery } from "@tanstack/react-query";

import { getApplicationContacts } from "../api/getApplicationContacts";

export function useApplicationContacts(applicationId: string) {
  return useQuery({
    queryKey: ["applications", applicationId, "contacts"],
    queryFn: () => getApplicationContacts(applicationId),
  });
}
