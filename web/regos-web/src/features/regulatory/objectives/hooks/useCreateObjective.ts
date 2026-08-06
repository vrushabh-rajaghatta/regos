import { useMutation, useQueryClient } from "@tanstack/react-query";

import {
  createObjective,
  type CreateObjectiveRequest,
} from "../api/createObjective";

export function useCreateObjective() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (request: CreateObjectiveRequest) => createObjective(request),

    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["objectives"] });
    },
  });
}
