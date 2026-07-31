import { useMutation, useQueryClient } from "@tanstack/react-query";

import { removeOrganizationIdentifier } from "../api/removeOrganizationIdentifier";

export function useRemoveOrganizationIdentifier(organizationId: string) {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (identifierId: string) =>
      removeOrganizationIdentifier(organizationId, identifierId),

    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["organizations"] });
    },
  });
}
