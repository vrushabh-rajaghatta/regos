import { useMutation, useQueryClient } from "@tanstack/react-query";

import { changeMarketStatus } from "../api/changeMarketStatus";
import type { ChangeMarketStatusBody } from "../api/changeMarketStatus";

export function useChangeMarketStatus(medicinalProductId: string) {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (body: ChangeMarketStatusBody) =>
      changeMarketStatus(medicinalProductId, body),

    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["medicinal-products"] });
    },
  });
}
