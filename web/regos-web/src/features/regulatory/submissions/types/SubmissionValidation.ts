export type ValidationSeverity = "Information" | "Warning" | "Error";

export interface ValidationIssue {
  code: string;
  message: string;
  severity: ValidationSeverity;
}

export interface SubmissionValidationResult {
  isValid: boolean;
  issues: ValidationIssue[];
}
