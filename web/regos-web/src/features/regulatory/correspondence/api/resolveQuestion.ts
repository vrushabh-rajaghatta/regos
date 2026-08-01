import { apiFetch, buildUrl } from "@/shared/api/apiClient";
import { detailOf } from "@/shared/api/problemDetail";

export async function resolveQuestion(
  correspondenceId: string,
  questionId: string,
  occurredOn: string,
): Promise<void> {
  const response = await apiFetch(
    buildUrl(
      `/api/correspondence/${correspondenceId}/questions/${questionId}/resolution`,
    ),
    {
      method: "POST",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify({ occurredOn }),
    },
  );

  if (!response.ok) {
    throw new Error(await detailOf(response, "Unable to resolve this question."));
  }
}
