import { apiFetch, buildUrl } from "@/shared/api/apiClient";

export async function deactivateUser(userId: string): Promise<void> {
  const response = await apiFetch(
    buildUrl(`/api/platform/users/${userId}/deactivate`),
    { method: "POST" },
  );

  if (response.ok) return;

  let message = "Unable to deactivate this user.";

  try {
    const problem = await response.json();

    if (typeof problem?.detail === "string") {
      message = problem.detail;
    }
  } catch {
    // No problem body - fall back to the generic message.
  }

  throw new Error(message);
}
