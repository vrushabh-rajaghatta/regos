import { useMutation } from "@tanstack/react-query";

import { acceptInvitation } from "../api/acceptInvitation";

export function useAcceptInvitation() {
  return useMutation({ mutationFn: acceptInvitation });
}
