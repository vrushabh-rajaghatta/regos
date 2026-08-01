import { apiFetch, buildUrl } from "@/shared/api/apiClient";
import { detailOf } from "@/shared/api/problemDetail";

export interface RespondToQuestionBody {
  responseText: string;
  occurredOn: string;
  note?: string | null;
}

export async function respondToQuestion(
  correspondenceId: string,
  questionId: string,
  body: RespondToQuestionBody,
): Promise<void> {
  const response = await apiFetch(
    buildUrl(
      `/api/correspondence/${correspondenceId}/questions/${questionId}/response`,
    ),
    {
      method: "POST",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify(body),
    },
  );

  if (!response.ok) {
    throw new Error(await detailOf(response, "Unable to record this response."));
  }
}
