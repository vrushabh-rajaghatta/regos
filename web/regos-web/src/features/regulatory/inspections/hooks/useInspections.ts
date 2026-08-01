import { useQuery } from "@tanstack/react-query";

import { listInspections } from "../api/listInspections";

export function useInspections(includeConcluded: boolean) {
  return useQuery({
    queryKey: ["inspections", includeConcluded],
    queryFn: () => listInspections(includeConcluded),
  });
}
