import { useMutation, useQueryClient } from "@tanstack/react-query";

import {
  createOrganizationSite,
  type CreateOrganizationSiteRequest,
} from "../api/createOrganizationSite";

export function useCreateOrganizationSite(organizationId: string) {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (request: CreateOrganizationSiteRequest) =>
      createOrganizationSite(organizationId, request),

    // Both the organization's list and the tenant-wide directory now have a
    // new row in them.
    onSuccess: () => {
      queryClient.invalidateQueries({
        queryKey: ["organizations", organizationId, "sites"],
      });

      queryClient.invalidateQueries({ queryKey: ["site-directory"] });
    },
  });
}
