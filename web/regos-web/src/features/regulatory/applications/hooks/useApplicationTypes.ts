import { useQuery } from "@tanstack/react-query";

import { listApplicationTypes } from "../api/listApplicationTypes";

export function useApplicationTypes(authorityId: string) {
  return useQuery({
    queryKey: ["application-types", authorityId],
    queryFn: () => listApplicationTypes(authorityId),
    enabled: !!authorityId,
  });
}
