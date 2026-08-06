import { useMutation, useQueryClient } from "@tanstack/react-query";

import {
  instantiatePlan,
  type InstantiatePlanRequest,
} from "../api/instantiatePlan";

export function useInstantiatePlan(objectiveId: string) {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (request: InstantiatePlanRequest) => instantiatePlan(request),

    onSuccess: () => {
      queryClient.invalidateQueries({
        queryKey: ["objectives", objectiveId, "plans"],
      });
    },
  });
}
