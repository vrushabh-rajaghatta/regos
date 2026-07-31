import { useMutation, useQueryClient } from "@tanstack/react-query";

import { createRegistration } from "../api/createRegistration";
import type { CreateRegistrationBody } from "../api/createRegistration";

export function useCreateRegistration(globalProductId: string) {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (body: CreateRegistrationBody) =>
      createRegistration(globalProductId, body),

    onSuccess: () => {
      // A new registration changes both portfolio axes and the market index —
      // a country we held nothing in a moment ago may now be on the list.
      queryClient.invalidateQueries({ queryKey: ["registrations"] });
    },
  });
}
