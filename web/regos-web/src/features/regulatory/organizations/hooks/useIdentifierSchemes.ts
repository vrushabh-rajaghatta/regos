import { useQuery } from "@tanstack/react-query";

import { listIdentifierSchemes } from "../api/listIdentifierSchemes";

/**
 * World facts, not a tenant's list — so they are cached generously. A registry
 * is not created between two page loads.
 */
export function useIdentifierSchemes() {
  return useQuery({
    queryKey: ["reference-data", "identifier-schemes"],
    queryFn: listIdentifierSchemes,
    staleTime: 60 * 60 * 1000,
  });
}
