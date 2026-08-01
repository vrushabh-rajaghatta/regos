import { useMutation, useQueryClient } from "@tanstack/react-query";

import { resolveQuestion } from "../api/resolveQuestion";

export function useResolveQuestion(
  correspondenceId: string,
  questionId: string,
) {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (occurredOn: string) =>
      resolveQuestion(correspondenceId, questionId, occurredOn),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["correspondence"] });
    },
  });
}
