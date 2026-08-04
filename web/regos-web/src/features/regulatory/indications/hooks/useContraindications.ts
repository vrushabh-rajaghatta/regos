import { useQuery } from "@tanstack/react-query";

import { listContraindications } from "../api/listStatements";

export function useContraindications(medicinalProductId: string) {
  return useQuery({
    queryKey: ["contraindications", medicinalProductId],
    queryFn: () => listContraindications(medicinalProductId),
    enabled: !!medicinalProductId,
  });
}
