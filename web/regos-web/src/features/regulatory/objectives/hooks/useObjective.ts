import { useQuery } from "@tanstack/react-query";

import { getObjective } from "../api/getObjective";

export function useObjective(id: string | undefined) {
  return useQuery({
    queryKey: ["objectives", id],
    queryFn: () => getObjective(id!),
    enabled: Boolean(id),
  });
}
