import { useMutation, useQueryClient } from "@tanstack/react-query";

import { withdrawSiteApproval } from "../api/withdrawSiteApproval";

export function useWithdrawSiteApproval(medicinalProductId: string) {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (siteApprovalId: string) =>
      withdrawSiteApproval(siteApprovalId),

    onSuccess: () => {
      queryClient.invalidateQueries({
        queryKey: ["manufacturing", medicinalProductId],
      });
    },
  });
}
