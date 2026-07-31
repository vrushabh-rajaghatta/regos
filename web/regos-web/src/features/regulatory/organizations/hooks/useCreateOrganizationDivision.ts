import { useMutation, useQueryClient } from "@tanstack/react-query";

import {
  createOrganizationDivision,
  type CreateOrganizationDivisionRequest,
} from "../api/createOrganizationDivision";

export function useCreateOrganizationDivision(organizationId: string) {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (request: CreateOrganizationDivisionRequest) =>
      createOrganizationDivision(organizationId, request),

    onSuccess: () => {
      queryClient.invalidateQueries({
        queryKey: ["organizations", organizationId, "divisions"],
      });
    },
  });
}
