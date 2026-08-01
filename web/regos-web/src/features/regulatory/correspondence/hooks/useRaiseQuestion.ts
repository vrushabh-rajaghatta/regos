import { useMutation, useQueryClient } from "@tanstack/react-query";

import { raiseQuestion, type RaiseQuestionBody } from "../api/raiseQuestion";

export function useRaiseQuestion(correspondenceId: string) {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (body: RaiseQuestionBody) =>
      raiseQuestion(correspondenceId, body),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["correspondence"] });
    },
  });
}
