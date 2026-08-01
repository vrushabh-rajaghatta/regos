import { apiFetch, buildUrl } from "@/shared/api/apiClient";
import { detailOf } from "@/shared/api/problemDetail";

export interface RaiseQuestionBody {
  number: string;
  text: string;
  targetResponseOn?: string | null;
}

export async function raiseQuestion(
  correspondenceId: string,
  body: RaiseQuestionBody,
): Promise<{ id: string }> {
  const response = await apiFetch(
    buildUrl(`/api/correspondence/${correspondenceId}/questions`),
    {
      method: "POST",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify(body),
    },
  );

  if (!response.ok) {
    throw new Error(await detailOf(response, "Unable to raise this question."));
  }

  return response.json();
}
