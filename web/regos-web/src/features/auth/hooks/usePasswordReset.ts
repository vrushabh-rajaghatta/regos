import { useMutation } from "@tanstack/react-query";

import { completePasswordReset } from "../api/completePasswordReset";
import { requestPasswordReset } from "../api/requestPasswordReset";

export function useRequestPasswordReset() {
  return useMutation({ mutationFn: requestPasswordReset });
}

export function useCompletePasswordReset() {
  return useMutation({ mutationFn: completePasswordReset });
}
