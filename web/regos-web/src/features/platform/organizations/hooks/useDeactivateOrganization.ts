import { useMutation, useQueryClient } from "@tanstack/react-query";

import { deactivateOrganization } from "../api/deactivateOrganization";

export function useDeactivateOrganization(organizationId: string) {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: () => deactivateOrganization(organizationId),

    // Re-read the directory so the status badge reflects the source of truth.
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["organizations"] });
    },
  });
}
