/**
 * What a filing's format is called on screen.
 *
 * The domain says `Ectd`, `Nees`, `Paper` — C# enum names. A regulatory user
 * says **eCTD** and **NeeS**, with the capitalisation the specifications
 * themselves use, and neither word may reach the other's side (ADR-047).
 *
 * This is the only place the mapping exists. It is casing rather than a
 * different word, so no vocabulary pair is recorded in `docs/domain-model/` —
 * but the boundary is real, and a component that writes "eCTD" into a request
 * body has crossed it.
 */
export const SUBMISSION_FORMATS = ["Ectd", "Nees", "Paper"] as const;

export type SubmissionFormat = (typeof SUBMISSION_FORMATS)[number];

const LABELS: Record<SubmissionFormat, string> = {
  Ectd: "eCTD",
  Nees: "NeeS",
  Paper: "Paper",
};

/**
 * Falls back to the server's own word rather than rendering nothing: a format
 * this client does not know about is a deployment skew, and showing the raw
 * value tells the user more than a blank does.
 */
export function formatLabel(format: string): string {
  return LABELS[format as SubmissionFormat] ?? format;
}

/** What each format means for the package that leaves RegOS. */
export const FORMAT_DESCRIPTIONS: Record<SubmissionFormat, string> = {
  Ectd: "Electronic, with an XML backbone and leaf-level lifecycle.",
  Nees: "Electronic, with no backbone — the changes travel as a cover letter.",
  Paper: "Filed on paper.",
};
