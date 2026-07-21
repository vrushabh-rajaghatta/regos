import { apiFetch, buildUrl } from "@/shared/api/apiClient";

export async function archiveProductDocument(
  productId: string,
  documentId: string
): Promise<void> {
  const response = await apiFetch(
    buildUrl(
      `/api/products/${productId}/documents/${documentId}/archive`
    ),
    { method: "POST" }
  );

  if (!response.ok) {
    throw new Error(await extractErrorMessage(response));
  }
}

async function extractErrorMessage(response: Response): Promise<string> {
  try {
    const problem = await response.json();

    if (problem && typeof problem.detail === "string") {
      return problem.detail;
    }
  } catch {
    // Body was not JSON; fall through to the generic message.
  }

  return "Failed to archive Document.";
}
