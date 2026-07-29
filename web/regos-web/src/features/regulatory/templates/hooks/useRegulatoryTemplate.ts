import { useQuery } from "@tanstack/react-query";

import { getRegulatoryTemplate } from "../api/getRegulatoryTemplate";

export function useRegulatoryTemplate(id: string | undefined) {
  return useQuery({
    queryKey: ["reference-data", "templates", id],
    queryFn: () => getRegulatoryTemplate(id!),
    enabled: !!id,
  });
}
