import { useQuery } from "@tanstack/react-query";

import { listObjectivePlans } from "../api/listObjectivePlans";

export function useObjectivePlans(objectiveId: string | undefined) {
  return useQuery({
    queryKey: ["objectives", objectiveId, "plans"],
    queryFn: () => listObjectivePlans(objectiveId!),
    enabled: Boolean(objectiveId),
  });
}
