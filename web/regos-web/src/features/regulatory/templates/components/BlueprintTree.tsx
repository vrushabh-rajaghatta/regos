import type {
  RegulatoryTemplateVersion,
  RequiredDocument,
  TemplateSection,
  ValidationRule,
} from "../types/RegulatoryTemplateDetail";
import { BlueprintSectionNode } from "./BlueprintSectionNode";
import { RuleRow } from "./RuleRow";

interface BlueprintTreeProps {
  version: RegulatoryTemplateVersion;
  documentTypeName: (id: string) => string;
}

/**
 * Renders a version's blueprint end-to-end. The API returns sections, required
 * documents and validation rules as flat lists; this component reassembles the
 * tree client-side — grouping children by parent, documents and section-scoped
 * rules by section — and surfaces the version-wide rules (those with no
 * section) in a block of their own above the tree.
 */
export function BlueprintTree({ version, documentTypeName }: BlueprintTreeProps) {
  const childrenByParent = new Map<string | null, TemplateSection[]>();
  for (const section of version.sections) {
    const siblings = childrenByParent.get(section.parentSectionId) ?? [];
    siblings.push(section);
    childrenByParent.set(section.parentSectionId, siblings);
  }
  for (const siblings of childrenByParent.values()) {
    siblings.sort((a, b) => a.order - b.order);
  }

  const docsBySection = new Map<string, RequiredDocument[]>();
  for (const doc of [...version.requiredDocuments].sort(
    (a, b) => a.order - b.order,
  )) {
    const list = docsBySection.get(doc.sectionId) ?? [];
    list.push(doc);
    docsBySection.set(doc.sectionId, list);
  }

  const rulesBySection = new Map<string, ValidationRule[]>();
  const versionRules: ValidationRule[] = [];
  for (const rule of [...version.validationRules].sort(
    (a, b) => a.order - b.order,
  )) {
    if (rule.sectionId === null) {
      versionRules.push(rule);
      continue;
    }
    const list = rulesBySection.get(rule.sectionId) ?? [];
    list.push(rule);
    rulesBySection.set(rule.sectionId, list);
  }

  const roots = childrenByParent.get(null) ?? [];

  return (
    <div className="space-y-6">
      {versionRules.length > 0 && (
        <div
          className="space-y-1.5 rounded-lg border bg-muted/30 p-4"
          data-testid="blueprint-version-rules"
        >
          <h3 className="text-sm font-medium">Blueprint-wide rules</h3>
          {versionRules.map((rule) => (
            <RuleRow key={rule.id} rule={rule} />
          ))}
        </div>
      )}

      <div className="rounded-lg border p-4" data-testid="blueprint-tree">
        {roots.map((section) => (
          <BlueprintSectionNode
            key={section.id}
            section={section}
            childrenByParent={childrenByParent}
            docsBySection={docsBySection}
            rulesBySection={rulesBySection}
            documentTypeName={documentTypeName}
          />
        ))}
      </div>
    </div>
  );
}
