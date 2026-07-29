import { Badge } from "@/components/ui/badge";

import type {
  RequiredDocument,
  TemplateSection,
  ValidationRule,
} from "../types/RegulatoryTemplateDetail";
import { RuleRow } from "./RuleRow";

interface BlueprintSectionNodeProps {
  section: TemplateSection;
  childrenByParent: Map<string | null, TemplateSection[]>;
  docsBySection: Map<string, RequiredDocument[]>;
  rulesBySection: Map<string, ValidationRule[]>;
  documentTypeName: (id: string) => string;
}

/**
 * One node in the dossier tree: its code and title, the documents it expects,
 * the rules scoped to it, and — nested and indented — its child sections.
 * Recurses through the whole subtree.
 */
export function BlueprintSectionNode({
  section,
  childrenByParent,
  docsBySection,
  rulesBySection,
  documentTypeName,
}: BlueprintSectionNodeProps) {
  const children = childrenByParent.get(section.id) ?? [];
  const docs = docsBySection.get(section.id) ?? [];
  const rules = rulesBySection.get(section.id) ?? [];

  return (
    <div data-testid="blueprint-section">
      <div className="py-1.5">
        <div className="flex items-baseline gap-2">
          <span className="font-mono text-xs text-muted-foreground">
            {section.code}
          </span>
          <span className="text-sm font-medium">{section.title}</span>
        </div>

        {(docs.length > 0 || rules.length > 0) && (
          <div className="mt-1.5 space-y-1.5 pl-1">
            {docs.map((doc) => (
              <div
                key={doc.id}
                className="flex items-center gap-2 text-sm"
                data-testid="required-document"
              >
                <Badge variant={doc.isMandatory ? "default" : "secondary"}>
                  {doc.isMandatory ? "Required" : "Optional"}
                </Badge>
                <span>{documentTypeName(doc.documentTypeId)}</span>
              </div>
            ))}

            {rules.map((rule) => (
              <RuleRow key={rule.id} rule={rule} />
            ))}
          </div>
        )}
      </div>

      {children.length > 0 && (
        <div className="ml-3 border-l pl-3">
          {children.map((child) => (
            <BlueprintSectionNode
              key={child.id}
              section={child}
              childrenByParent={childrenByParent}
              docsBySection={docsBySection}
              rulesBySection={rulesBySection}
              documentTypeName={documentTypeName}
            />
          ))}
        </div>
      )}
    </div>
  );
}
