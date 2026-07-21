import { useMutation, useQueryClient } from "@tanstack/react-query";

import { activateOrganization } from "../api/activateOrganization";

export function useActivateOrganization(organizationId: string) {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: () => activateOrganization(organizationId),

    // Re-read the directory so the status badge reflects the source of truth.
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["organizations"] });
    },
  });
}
