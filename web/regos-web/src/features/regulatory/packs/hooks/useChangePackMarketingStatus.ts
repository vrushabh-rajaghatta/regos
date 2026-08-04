import { useMutation, useQueryClient } from "@tanstack/react-query";

import {
  changePackMarketingStatus,
  type ChangePackMarketingStatusBody,
} from "../api/changePackMarketingStatus";

export function useChangePackMarketingStatus(medicinalProductId: string) {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: ({
      packagedProductId,
      body,
    }: {
      packagedProductId: string;
      body: ChangePackMarketingStatusBody;
    }) => changePackMarketingStatus(packagedProductId, body),

    onSuccess: () => {
      queryClient.invalidateQueries({
        queryKey: ["packs", medicinalProductId],
      });
    },
  });
}
