import { useMutation, useQueryClient } from "@tanstack/react-query";

import { changeRegistrationStatus } from "../api/changeRegistrationStatus";
import type { ChangeStatusBody } from "../api/changeRegistrationStatus";

export function useChangeRegistrationStatus(registrationId: string) {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (body: ChangeStatusBody) =>
      changeRegistrationStatus(registrationId, body),

    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["registrations"] });
    },
  });
}
