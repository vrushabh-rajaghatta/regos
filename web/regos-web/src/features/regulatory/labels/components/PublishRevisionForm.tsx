import { zodResolver } from "@hookform/resolvers/zod";
import { Controller, useForm } from "react-hook-form";

import { Button } from "@/components/ui/button";
import {
  Field,
  FieldError,
  FieldGroup,
  FieldLabel,
} from "@/components/ui/field";
import { Input } from "@/components/ui/input";

import { usePublishLocalLabelRevision } from "../hooks/usePublishLocalLabelRevision";
import {
  publishLocalLabelRevisionSchema,
  type PublishLocalLabelRevisionFormValues,
} from "../validation/publishLocalLabelRevisionSchema";

interface PublishRevisionFormProps {
  localLabelId: string;
  revisionId: string;
  onSuccess(): void;
}

/**
 * Puts a revision in force.
 *
 * **Two dates, always.** Approved 12 May and effective 1 June is as ordinary as
 * approved and effective the same day, and a form asking for one could not tell
 * them apart.
 */
export function PublishRevisionForm({
  localLabelId,
  revisionId,
  onSuccess,
}: PublishRevisionFormProps) {
  const mutation = usePublishLocalLabelRevision();

  const {
    control,
    handleSubmit,
    reset,
    formState: { errors },
  } = useForm<PublishLocalLabelRevisionFormValues>({
    resolver: zodResolver(publishLocalLabelRevisionSchema),
    defaultValues: { approvedOn: "", effectiveFrom: "" },
  });

  async function onSubmit(values: PublishLocalLabelRevisionFormValues) {
    try {
      await mutation.mutateAsync({ localLabelId, revisionId, ...values });
    } catch {
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
          name="approvedOn"
          render={({ field }) => (
            <Field data-invalid={!!errors.approvedOn}>
              <FieldLabel htmlFor="approvedOn">Approved on</FieldLabel>

              <Input id="approvedOn" type="date" {...field} />

              <p className="text-xs text-muted-foreground">
                The day the authority approved this revision.
              </p>

              <FieldError errors={[errors.approvedOn]} />
            </Field>
          )}
        />

        <Controller
          control={control}
          name="effectiveFrom"
          render={({ field }) => (
            <Field data-invalid={!!errors.effectiveFrom}>
              <FieldLabel htmlFor="effectiveFrom">Takes effect</FieldLabel>

              <Input id="effectiveFrom" type="date" {...field} />

              <p className="text-xs text-muted-foreground">
                May be the same day. The revision it replaces is retired the day
                before.
              </p>

              <FieldError errors={[errors.effectiveFrom]} />
            </Field>
          )}
        />
      </FieldGroup>

      {mutation.isError && (
        <p
          className="text-sm text-destructive"
          data-testid="publish-revision-error"
        >
          {(mutation.error as Error).message}
        </p>
      )}

      <div className="flex justify-end gap-2">
        <Button type="submit" disabled={mutation.isPending}>
          {mutation.isPending ? "Saving..." : "Put in force"}
        </Button>
      </div>
    </form>
  );
}
