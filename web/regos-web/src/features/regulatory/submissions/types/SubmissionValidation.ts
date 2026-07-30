export type ValidationSeverity = "Information" | "Warning" | "Error";

export interface ValidationIssue {
  code: string;
  message: string;
  severity: ValidationSeverity;
  /** The blueprint rule behind the issue (e.g. "FDA-IND-PDF"), when one produced it. */
  ruleCode?: string | null;
  /**
   * Blueprint rule types this validator cannot execute yet. Read this rather
   * than parsing the message.
   */
  unevaluatedRuleTypes?: string[] | null;
}

export interface SubmissionValidationResult {
  /** Whether anything *blocks* publishing — not whether issues exist. */
  isValid: boolean;
  /** Ordered by the API: most severe first, then by code. */
  issues: ValidationIssue[];
}
