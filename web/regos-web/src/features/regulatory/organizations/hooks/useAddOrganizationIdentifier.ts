import { useMutation, useQueryClient } from "@tanstack/react-query";

import {
  addOrganizationIdentifier,
  type AddOrganizationIdentifierRequest,
} from "../api/addOrganizationIdentifier";

export function useAddOrganizationIdentifier(organizationId: string) {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (request: AddOrganizationIdentifierRequest) =>
      addOrganizationIdentifier(organizationId, request),

    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["organizations"] });
    },
  });
}
