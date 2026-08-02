import { apiFetch, buildUrl } from "@/shared/api/apiClient";

export async function removeSubmissionRole(
  submissionId: string,
  roleId: string
): Promise<void> {
  const response = await apiFetch(
    buildUrl(`/api/submissions/${submissionId}/roles/${roleId}`),
    { method: "DELETE" }
  );

  if (!response.ok) {
    throw new Error("Failed to remove that naming.");
  }
}
