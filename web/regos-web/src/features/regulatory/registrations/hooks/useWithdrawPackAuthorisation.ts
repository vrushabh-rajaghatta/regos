import { useMutation, useQueryClient } from "@tanstack/react-query";

import { withdrawPackAuthorisation } from "../api/withdrawPackAuthorisation";

export function useWithdrawPackAuthorisation(medicinalProductId: string) {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (packAuthorisationId: string) =>
      withdrawPackAuthorisation(packAuthorisationId),

    onSuccess: () => {
      queryClient.invalidateQueries({
        queryKey: ["packs", medicinalProductId],
      });
    },
  });
}
