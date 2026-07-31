import { useQuery } from "@tanstack/react-query";

import { listMarketRegistrations } from "../api/listMarketRegistrations";

export function useMarketRegistrations(countryId: string) {
  return useQuery({
    queryKey: ["registrations", "market", countryId],
    queryFn: () => listMarketRegistrations(countryId),
    enabled: !!countryId,
  });
}
