import { useMutation, useQueryClient } from "@tanstack/react-query";

import { approveSite, type ApproveSiteBody } from "../api/approveSite";

export function useApproveSite(medicinalProductId: string) {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (body: ApproveSiteBody) => approveSite(body),

    onSuccess: () => {
      queryClient.invalidateQueries({
        queryKey: ["manufacturing", medicinalProductId],
      });
    },
  });
}
