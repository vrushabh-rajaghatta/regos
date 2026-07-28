import { useQuery } from "@tanstack/react-query";

import { listRegulatoryTemplates } from "../api/listRegulatoryTemplates";

export function useRegulatoryTemplates() {
  return useQuery({
    queryKey: ["reference-data", "templates"],
    queryFn: listRegulatoryTemplates,
  });
}
