import { useMutation, useQueryClient } from "@tanstack/react-query";

import {
  changeObjectiveStatus,
  type ChangeObjectiveStatusRequest,
} from "../api/changeObjectiveStatus";

export function useChangeObjectiveStatus(id: string) {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (request: ChangeObjectiveStatusRequest) =>
      changeObjectiveStatus(id, request),

    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["objectives"] });
    },
  });
}
