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

import { usePublishGlobalLabelVersion } from "../hooks/usePublishGlobalLabelVersion";
import {
  publishGlobalLabelVersionSchema,
  type PublishGlobalLabelVersionFormValues,
} from "../validation/publishGlobalLabelVersionSchema";

interface PublishVersionFormProps {
  globalLabelId: string;
  versionId: string;
  onSuccess(): void;
}

/**
 * Puts a draft in force.
 *
 * The date asked for is the day the label **takes effect**, not today — a
 * version approved in March to apply from June is the ordinary case, and a form
 * that assumed the clock would flatten the two.
 */
export function PublishVersionForm({
  globalLabelId,
  versionId,
  onSuccess,
}: PublishVersionFormProps) {
  const mutation = usePublishGlobalLabelVersion();

  const {
    control,
    handleSubmit,
    reset,
    formState: { errors },
  } = useForm<PublishGlobalLabelVersionFormValues>({
    resolver: zodResolver(publishGlobalLabelVersionSchema),
    defaultValues: { effectiveFrom: "", changeSummary: "" },
  });

  async function onSubmit(values: PublishGlobalLabelVersionFormValues) {
    try {
      await mutation.mutateAsync({
        globalLabelId,
        versionId,
        effectiveFrom: values.effectiveFrom,
        changeSummary: values.changeSummary?.trim() || null,
      });
    } catch {
      // The server refuses a date on or before the version in force, and a
      // draft with no document attached. Both render below, verbatim.
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
          name="effectiveFrom"
          render={({ field }) => (
            <Field data-invalid={!!errors.effectiveFrom}>
              <FieldLabel htmlFor="effectiveFrom">Takes effect</FieldLabel>

              <Input id="effectiveFrom" type="date" {...field} />

              <p className="text-xs text-muted-foreground">
                The day this issue becomes the one in force. The version it
                replaces is retired the day before.
              </p>

              <FieldError errors={[errors.effectiveFrom]} />
            </Field>
          )}
        />

        <Controller
          control={control}
          name="changeSummary"
          render={({ field }) => (
            <Field data-invalid={!!errors.changeSummary}>
              <FieldLabel htmlFor="changeSummary">What changed</FieldLabel>

              <Input id="changeSummary" placeholder="Optional" {...field} />

              <FieldError errors={[errors.changeSummary]} />
            </Field>
          )}
        />
      </FieldGroup>

      {mutation.isError && (
        <p className="text-sm text-destructive" data-testid="publish-error">
          {(mutation.error as Error).message}
        </p>
      )}

      <div className="flex justify-end gap-2">
        <Button type="submit" disabled={mutation.isPending}>
          {mutation.isPending ? "Publishing..." : "Publish version"}
        </Button>
      </div>
    </form>
  );
}
