import { apiFetch, buildUrl } from "@/shared/api/apiClient";

export interface FileTagOption {
  name: string;
  /** `ich`, `us` or `jp` — surfaced so a filer can see a tag is regional. */
  realm: string;
}

/**
 * ICH's published `file-tag` vocabulary — 97 values.
 *
 * Not under `/reference-data`: nothing here is seeded, it is a table in code
 * checked against the held `valid-values.xml`.
 */
export async function listFileTags(): Promise<FileTagOption[]> {
  const response = await apiFetch(buildUrl("/api/study-tagging/file-tags"));

  if (!response.ok) {
    throw new Error("Unable to load the file tag vocabulary.");
  }

  return response.json();
}
