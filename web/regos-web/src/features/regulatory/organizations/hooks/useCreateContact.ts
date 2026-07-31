import { useMutation, useQueryClient } from "@tanstack/react-query";

import {
  createContact,
  type CreateContactRequest,
} from "../api/createContact";

export function useCreateContact(organizationId: string) {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (request: CreateContactRequest) =>
      createContact(organizationId, request),

    onSuccess: () => {
      queryClient.invalidateQueries({
        queryKey: ["organizations", organizationId, "contacts"],
      });

      queryClient.invalidateQueries({ queryKey: ["contact-directory"] });
    },
  });
}
