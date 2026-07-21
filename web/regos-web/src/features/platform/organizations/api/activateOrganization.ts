import { buildUrl } from "@/shared/api/apiClient";

export async function activateOrganization(
  organizationId: string,
): Promise<void> {
  const response = await fetch(
    buildUrl(`/organizations/${organizationId}/activate`),
    { method: "POST" },
  );

  if (response.ok) return;

  let message = "Unable to activate this organization.";

  try {
    const problem = await response.json();

    if (typeof problem?.detail === "string") {
      message = problem.detail;
    }
  } catch {
    // No problem body — fall back to the generic message.
  }

  throw new Error(message);
}
