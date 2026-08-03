import { useMutation, useQueryClient } from "@tanstack/react-query";

import { registerStudy } from "../api/registerStudy";
import type { RegisterStudyBody } from "../api/registerStudy";
import type { StudyKind } from "../types/Study";

export function useRegisterStudy() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (input: RegisterStudyBody & { kind: StudyKind }) =>
      registerStudy(input.kind, {
        sponsorStudyIdentifier: input.sponsorStudyIdentifier,
        title: input.title,
      }),

    // One list over both kinds, so one key to invalidate whichever was added.
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["studies"] });
    },
  });
}
