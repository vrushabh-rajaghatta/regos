import { useQuery } from "@tanstack/react-query";

import { listExpiringRegistrations } from "../api/listExpiringRegistrations";

export function useExpiringRegistrations() {
  return useQuery({
    queryKey: ["registrations", "expiring"],
    queryFn: listExpiringRegistrations,
  });
}
