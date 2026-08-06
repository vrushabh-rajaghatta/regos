import { useState } from "react";

import { Button } from "@/components/ui/button";
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@/components/ui/select";
import { useObjectives } from "@/features/regulatory/objectives/hooks/useObjectives";
import { useObjectivePlans } from "@/features/regulatory/plans/hooks/useObjectivePlans";
import { usePlan } from "@/features/regulatory/plans/hooks/usePlan";

import { useAttachCorrespondenceToStep } from "../hooks/useAttachCorrespondenceToStep";

interface CorrespondencePlanStepSectionProps {
  correspondenceId: string;
  /** The step this letter already serves, if any. */
  processStepId: string | null;
}

/**
 * Which planned work this letter serves.
 *
 * **Optional, and its absence means nothing.** A letter with no step is
 * complete, valid and unremarkable — most correspondence was filed before
 * anyone planned anything here. Linking one changes what is discoverable and
 * nothing else: it does not complete the step, and completing the step will
 * never send the letter.
 *
 * **The command lives here rather than in the plan UI** because the letter owns
 * the column. That asymmetry is deliberate — reads compose across contexts,
 * writes stay with the aggregate that owns the record.
 *
 * **Correspondence is the only interaction with this control**, and that is a
 * missing screen rather than a missing rule: meetings, inspections and
 * commitments have the same API and no detail page to put it on. Adding the
 * control to the plan page instead would make Process the place users manage
 * other contexts' records, which is exactly what ADR-065 D2 refuses.
 */
export function CorrespondencePlanStepSection({
  correspondenceId,
  processStepId,
}: CorrespondencePlanStepSectionProps) {
  const [objectiveId, setObjectiveId] = useState("");
  const [planId, setPlanId] = useState("");

  const attach = useAttachCorrespondenceToStep(correspondenceId);

  const { data: objectives } = useObjectives();
  const { data: plans } = useObjectivePlans(objectiveId || undefined);
  const { data: plan } = usePlan(planId || undefined);

  const linked = plan?.steps.find((step) => step.id === processStepId);

  return (
    <section className="mt-8" data-testid="correspondence-plan-step">
      <h2 className="text-sm font-medium">Planned work</h2>

      <p className="mt-1 max-w-prose text-sm text-muted-foreground">
        {processStepId
          ? "This correspondence is recorded as serving a step of a plan."
          : "Not linked to a plan. That is not a gap — a letter is complete on its own, and linking one only makes it discoverable from the plan."}
      </p>

      {processStepId && (
        <div className="mt-3 rounded-lg border p-3 text-sm">
          <span className="font-medium">{linked?.name ?? "Linked step"}</span>

          {linked && (
            <span className="ml-2 font-mono text-xs text-muted-foreground">
              {linked.code}
            </span>
          )}

          <Button
            variant="ghost"
            size="sm"
            className="ml-3"
            data-testid="detach-step"
            disabled={attach.isPending}
            onClick={() => attach.mutate(null)}
          >
            Unlink
          </Button>
        </div>
      )}

      {!processStepId && (
        <div className="mt-3 flex flex-wrap gap-2">
          <Select
            onValueChange={(value) => setObjectiveId(value ?? "")}
            value={objectiveId}
          >
            <SelectTrigger className="w-56">
              <SelectValue placeholder="Objective" />
            </SelectTrigger>

            <SelectContent>
              {objectives?.map((objective) => (
                <SelectItem key={objective.id} value={objective.id}>
                  {objective.name}
                </SelectItem>
              ))}
            </SelectContent>
          </Select>

          <Select
            onValueChange={(value) => setPlanId(value ?? "")}
            value={planId}
            disabled={!plans || plans.length === 0}
          >
            <SelectTrigger className="w-56">
              <SelectValue placeholder="Plan" />
            </SelectTrigger>

            <SelectContent>
              {plans?.map((entry) => (
                <SelectItem key={entry.id} value={entry.id}>
                  {entry.name}
                </SelectItem>
              ))}
            </SelectContent>
          </Select>

          <Select
            onValueChange={(stepId) => stepId && attach.mutate(String(stepId))}
            disabled={!plan}
          >
            <SelectTrigger className="w-56">
              <SelectValue placeholder="Step" />
            </SelectTrigger>

            <SelectContent>
              {plan?.steps.map((step) => (
                <SelectItem key={step.id} value={step.id}>
                  {step.code} — {step.name}
                </SelectItem>
              ))}
            </SelectContent>
          </Select>
        </div>
      )}

      {attach.isError && (
        <p className="mt-2 text-sm text-destructive" role="alert">
          {attach.error.message}
        </p>
      )}
    </section>
  );
}
