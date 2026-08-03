import { apiFetch, buildUrl } from "@/shared/api/apiClient";

import { detailOf } from "@/shared/api/problemDetail";

export interface GeneratedPackage {
  fileName: string;
  blob: Blob;
}

/**
 * Asks the server to build the eCTD package for a published sequence.
 *
 * The server refuses in five distinct ways — a sequence that predates the
 * activity model, an unread wire vocabulary, a fact the domain does not hold, a
 * business fact nobody has entered, and a path the region will not accept — and
 * each says what to do next. The message is shown verbatim rather than
 * summarised, because summarising it is what would collapse them.
 */
export async function generateSequencePackage(
  submissionId: string,
): Promise<GeneratedPackage> {
  const response = await apiFetch(
    buildUrl(`/api/submissions/${submissionId}/package`),
    { method: "POST" },
  );

  if (!response.ok) {
    throw new Error(
      await detailOf(response, "Unable to generate this package."),
    );
  }

  const disposition = response.headers.get("content-disposition") ?? "";
  const named = /filename="?([^";]+)"?/.exec(disposition);

  return {
    fileName: named?.[1] ?? "package.zip",
    blob: await response.blob(),
  };
}
