import { useMutation, useQueryClient } from "@tanstack/react-query";

import { updateOrganization } from "../api/updateOrganization";
import type { UpdateOrganizationRequest } from "../types/UpdateOrganizationRequest";

export function useUpdateOrganization(organizationId: string) {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (request: UpdateOrganizationRequest) =>
      updateOrganization(organizationId, request),

    // Invalidating the prefix refreshes both this organization and the
    // directory, so the list reflects the edit without a manual reload.
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["organizations"] });
    },
  });
}
