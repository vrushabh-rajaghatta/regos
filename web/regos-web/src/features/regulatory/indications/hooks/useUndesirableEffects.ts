import { useQuery } from "@tanstack/react-query";

import { listUndesirableEffects } from "../api/listStatements";

export function useUndesirableEffects(medicinalProductId: string) {
  return useQuery({
    queryKey: ["undesirable-effects", medicinalProductId],
    queryFn: () => listUndesirableEffects(medicinalProductId),
    enabled: !!medicinalProductId,
  });
}
