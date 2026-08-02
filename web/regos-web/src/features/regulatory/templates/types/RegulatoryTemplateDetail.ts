export interface TemplateSection {
  id: string;
  code: string;
  title: string;
  parentSectionId: string | null;
  order: number;
}

export interface RequiredDocument {
  id: string;
  sectionId: string;
  documentTypeId: string;
  isMandatory: boolean;
  order: number;
}

export interface ValidationRule {
  id: string;
  code: string;
  ruleType: string;
  severity: string;
  // null => the rule applies to the whole version, not one section.
  sectionId: string | null;
  parameters: string | null;
  message: string;
  order: number;
}

export interface RegulatoryTemplateVersion {
  id: string;
  versionNumber: number;
  status: string;
  effectiveFrom: string | null;
  effectiveTo: string | null;
  publishedOnUtc: string | null;
  sections: TemplateSection[];
  requiredDocuments: RequiredDocument[];
  validationRules: ValidationRule[];
}

export interface RegulatoryTemplateDetail {
  id: string;
  code: string;
  name: string;
  authorityId: string;
  applicationTypeId: string;
  source: string;
  status: string;
  versions: RegulatoryTemplateVersion[];
}
