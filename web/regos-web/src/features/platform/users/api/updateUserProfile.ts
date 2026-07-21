import { apiFetch, buildUrl } from "@/shared/api/apiClient";

export interface UpdateUserProfileRequest {
  firstName: string;
  lastName: string;
  email: string;
}

export async function updateUserProfile(
  userId: string,
  request: UpdateUserProfileRequest,
): Promise<void> {
  const response = await apiFetch(buildUrl(`/api/platform/users/${userId}`), {
    method: "PUT",
    headers: {
      "Content-Type": "application/json",
    },
    body: JSON.stringify(request),
  });

  // 204 No Content on success - nothing to parse.
  if (response.ok) return;

  // Surface the API's ProblemDetails (duplicate email, invalid input, missing
  // user) rather than a generic failure.
  let message = "Unable to save changes.";

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
