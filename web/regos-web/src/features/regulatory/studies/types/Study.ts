/**
 * A study as the registry lists it.
 *
 * `kind` says which aggregate the row came from — the API composes the list
 * across two of them (ADR-056), so the label belongs to the read rather than
 * to either study.
 */
export interface Study {
  id: string;
  kind: StudyKind;
  sponsorStudyIdentifier: string;
  title: string;
  createdOn: string;
}

export type StudyKind = "Clinical" | "NonClinical";
