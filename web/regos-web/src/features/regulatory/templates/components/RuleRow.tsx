import { Badge } from "@/components/ui/badge";

import type { ValidationRule } from "../types/RegulatoryTemplateDetail";

/**
 * One validation rule: its severity, the check it performs (with any
 * parameter), and the human-readable message. Used both for section-scoped
 * rules and for the blueprint-wide rules at the top of the tree.
 */
export function RuleRow({ rule }: { rule: ValidationRule }) {
  return (
    <div
      className="flex flex-wrap items-center gap-2 text-sm"
      data-testid="blueprint-rule"
    >
      <Badge variant={rule.severity === "Error" ? "destructive" : "outline"}>
        {rule.severity}
      </Badge>

      <span className="font-mono text-xs text-muted-foreground">
        {rule.ruleType}
        {rule.parameters ? `: ${rule.parameters}` : ""}
      </span>

      <span className="text-muted-foreground">{rule.message}</span>
    </div>
  );
}
