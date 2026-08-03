/**
 * The dossier as a working surface: the bound blueprint's sections, the
 * placeholders each expects, and what fills them.
 *
 * Every dossier semantic here is decided by the server — whether a placeholder
 * is satisfied, what counts as supporting content, how full the dossier is. The
 * UI composes this with the validation result; it never re-derives either.
 */
export interface ContentPlanDocument {
  submissionDocumentId: string;
  productDocumentId: string;
  name: string;
  documentTypeId: string;
  documentTypeName: string;
  versionNumber: number;
  fileName: string;
  /**
   * The study this *placement* reports, if any — never a fact about the
   * document, which can be filed again elsewhere and report something else.
   * Null is the ordinary case: outside CTD 4.2.x and 5.3.1.x–5.3.5.x a
   * document reports no study.
   */
  studyId: string | null;
  studyKind: "Clinical" | "NonClinical" | null;
  /** The sponsor's code for it — "Study ID" on screen. */
  studyIdentifier: string | null;
}

export interface ContentPlanPlaceholder {
  /** Stable across reads: the bound template version is immutable. */
  placeholderId: string;
  documentTypeId: string;
  documentTypeName: string;
  isMandatory: boolean;
  order: number;
  isSatisfied: boolean;
  documents: ContentPlanDocument[];
}

export interface ContentPlanSection {
  sectionId: string;
  code: string;
  title: string;
  order: number;
  placeholders: ContentPlanPlaceholder[];
  /** Placed here, satisfying no placeholder — dossier content, not a finding. */
  additionalDocuments: ContentPlanDocument[];
  children: ContentPlanSection[];
}

export interface ContentPlanProgress {
  placeholders: number;
  satisfied: number;
  mandatory: number;
  mandatorySatisfied: number;
}

export interface SubmissionContentPlan {
  submissionId: string;
  /** Null when no published blueprint governs this submission. */
  boundTemplateVersionId: string | null;
  templateName: string | null;
  versionNumber: number | null;
  progress: ContentPlanProgress;
  sections: ContentPlanSection[];
  /** Attached, but nowhere in the dossier — so satisfying nothing. */
  unplacedDocuments: ContentPlanDocument[];
}
