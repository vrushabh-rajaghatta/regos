import { zodResolver } from "@hookform/resolvers/zod";
import { Controller, useForm, useWatch } from "react-hook-form";

import { Button } from "@/components/ui/button";
import {
  Field,
  FieldError,
  FieldGroup,
  FieldLabel,
} from "@/components/ui/field";
import { Input } from "@/components/ui/input";
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@/components/ui/select";
import { usePlaybook } from "@/features/regulatory/playbooks/hooks/usePlaybook";
import { usePlaybooks } from "@/features/regulatory/playbooks/hooks/usePlaybooks";

import { useInstantiatePlan } from "../hooks/useInstantiatePlan";
import {
  instantiatePlanSchema,
  type InstantiatePlanValues,
} from "../validation/instantiatePlanSchema";

interface InstantiatePlanFormProps {
  objectiveId: string;
  onSuccess(): void;
}

/**
 * A playbook, one of its **published versions**, and a date to schedule from.
 *
 * **The version is chosen, never resolved.** Picking "the latest" server-side
 * would make a plan's schedule depend on when it was created rather than on what
 * it was created from — and the whole point is that the answer to *"why is this
 * milestone on this date?"* is a version number and an anchor date.
 *
 * **Dates are derived once.** Nothing here recalculates afterwards: moving a
 * step later will move nothing else, by design.
 */
export function InstantiatePlanForm({
  objectiveId,
  onSuccess,
}: InstantiatePlanFormProps) {
  const mutation = useInstantiatePlan(objectiveId);

  const { data: playbooks } = usePlaybooks();

  const {
    control,
    handleSubmit,
    reset,
    formState: { errors },
  } = useForm<InstantiatePlanValues>({
    resolver: zodResolver(instantiatePlanSchema),
    defaultValues: {
      processDefinitionId: "",
      processDefinitionVersionId: "",
      anchorDate: "",
      name: "",
    },
  });

  // useWatch, not watch(): the latter returns a function the React compiler
  // cannot memoize, and it is the one lint warning this form would otherwise
  // add to a baseline of three.
  const selectedPlaybook = useWatch({ control, name: "processDefinitionId" });

  const { data: playbook } = usePlaybook(selectedPlaybook || undefined);

  // Only published versions can be instantiated from. A draft is still being
  // written and a superseded one is no longer started from — both refused by the
  // server, and both hidden here so the refusal is rare rather than routine.
  const versions =
    playbook?.versions.filter((version) => version.status === "Published") ?? [];

  async function onSubmit(values: InstantiatePlanValues) {
    try {
      await mutation.mutateAsync({
        processObjectiveId: objectiveId,
        processDefinitionVersionId: values.processDefinitionVersionId,
        anchorDate: values.anchorDate,
        name: values.name,
      });
    } catch {
      // A refusal is an outcome, not a crash.
      return;
    }

    reset();

    onSuccess();
  }

  return (
    <form onSubmit={handleSubmit(onSubmit)} className="space-y-6">
      <FieldGroup>
        <Controller
          control={control}
          name="processDefinitionId"
          render={({ field }) => (
            <Field data-invalid={!!errors.processDefinitionId}>
              <FieldLabel htmlFor="processDefinitionId">Playbook</FieldLabel>

              <Select onValueChange={field.onChange} value={field.value}>
                <SelectTrigger id="processDefinitionId">
                  <SelectValue placeholder="Select a playbook" />
                </SelectTrigger>

                <SelectContent>
                  {playbooks?.map((entry) => (
                    <SelectItem key={entry.id} value={entry.id}>
                      {entry.name}
                    </SelectItem>
                  ))}
                </SelectContent>
              </Select>

              <FieldError errors={[errors.processDefinitionId]} />
            </Field>
          )}
        />

        <Controller
          control={control}
          name="processDefinitionVersionId"
          render={({ field }) => (
            <Field data-invalid={!!errors.processDefinitionVersionId}>
              <FieldLabel htmlFor="processDefinitionVersionId">
                Version
              </FieldLabel>

              <Select
                onValueChange={field.onChange}
                value={field.value}
                disabled={versions.length === 0}
              >
                <SelectTrigger id="processDefinitionVersionId">
                  <SelectValue
                    placeholder={
                      selectedPlaybook
                        ? "Select a published version"
                        : "Choose a playbook first"
                    }
                  />
                </SelectTrigger>

                <SelectContent>
                  {versions.map((version) => (
                    <SelectItem key={version.id} value={version.id}>
                      v{version.versionNumber}
                      {version.effectiveFrom
                        ? ` — effective ${version.effectiveFrom}`
                        : ""}
                    </SelectItem>
                  ))}
                </SelectContent>
              </Select>

              <p className="text-xs text-muted-foreground">
                The plan stays on this version for good. Publishing a newer one
                later will not move any date.
              </p>

              <FieldError errors={[errors.processDefinitionVersionId]} />
            </Field>
          )}
        />

        <Controller
          control={control}
          name="anchorDate"
          render={({ field }) => (
            <Field data-invalid={!!errors.anchorDate}>
              <FieldLabel htmlFor="anchorDate">Start from</FieldLabel>

              <Input id="anchorDate" type="date" {...field} />

              <p className="text-xs text-muted-foreground">
                Every date is worked out once from this one.
              </p>

              <FieldError errors={[errors.anchorDate]} />
            </Field>
          )}
        />

        <Controller
          control={control}
          name="name"
          render={({ field }) => (
            <Field data-invalid={!!errors.name}>
              <FieldLabel htmlFor="name">Plan name</FieldLabel>

              <Input
                id="name"
                placeholder="e.g. US IND filing plan"
                {...field}
              />

              <FieldError errors={[errors.name]} />
            </Field>
          )}
        />
      </FieldGroup>

      {mutation.isError && (
        <p className="text-sm text-destructive" role="alert">
          {mutation.error.message}
        </p>
      )}

      <Button type="submit" disabled={mutation.isPending}>
        {mutation.isPending ? "Creating..." : "Create plan"}
      </Button>
    </form>
  );
}
