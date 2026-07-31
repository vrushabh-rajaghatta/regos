import { useQuery } from "@tanstack/react-query";

import { getRegistration } from "../api/getRegistration";

export function useRegistration(registrationId: string) {
  return useQuery({
    queryKey: ["registrations", registrationId],
    queryFn: () => getRegistration(registrationId),
    enabled: !!registrationId,
  });
}
