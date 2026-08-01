import { useMutation, useQueryClient } from "@tanstack/react-query";

import {
  respondToQuestion,
  type RespondToQuestionBody,
} from "../api/respondToQuestion";

export function useRespondToQuestion(
  correspondenceId: string,
  questionId: string,
) {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (body: RespondToQuestionBody) =>
      respondToQuestion(correspondenceId, questionId, body),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["correspondence"] });
    },
  });
}
