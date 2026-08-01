import { useQuery } from "@tanstack/react-query";

import { listAuthorityDivisions } from "../api/listAuthorityDivisions";

/**
 * Scoped to one authority, and disabled until one is chosen — an unscoped list
 * would offer a Health Canada bureau on an FDA letter, which the server refuses
 * anyway. The picker should not be able to compose the refusal.
 */
export function useAuthorityDivisions(authorityId: string) {
  return useQuery({
    queryKey: ["correspondence", "divisions", authorityId],
    queryFn: () => listAuthorityDivisions(authorityId),
    enabled: authorityId !== "",
    staleTime: 5 * 60 * 1000,
  });
}
