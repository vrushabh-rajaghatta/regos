import { useQuery } from "@tanstack/react-query";

import { listIndications } from "../api/listIndications";

export function useIndications(medicinalProductId: string) {
  return useQuery({
    queryKey: ["indications", medicinalProductId],
    queryFn: () => listIndications(medicinalProductId),
    enabled: !!medicinalProductId,
  });
}
