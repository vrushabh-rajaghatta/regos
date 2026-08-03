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

import { useRecordApplicationNumber } from "../hooks/useRecordApplicationNumber";
import {
  recordApplicationNumberSchema,
  type RecordApplicationNumberFormValues,
} from "../validation/recordApplicationNumberSchema";

interface RecordApplicationNumberFormProps {
  applicationId: string;
  currentNumber: string | null;
  onSuccess(): void;
}

/**
 * The number an authority assigned, entered as they issued it.
 *
 * No format is imposed here. FDA issues six digits and other authorities issue
 * something else entirely, so a client-side pattern would reject numbers the
 * API accepts — and would put one regulator's convention in front of every
 * user (ADR-055).
 */
export function RecordApplicationNumberForm({
  applicationId,
  currentNumber,
  onSuccess,
}: RecordApplicationNumberFormProps) {
  const mutation = useRecordApplicationNumber(applicationId);

  const {
    control,
    handleSubmit,
    formState: { errors },
  } = useForm<RecordApplicationNumberFormValues>({
    resolver: zodResolver(recordApplicationNumberSchema),
    defaultValues: { applicationNumber: currentNumber ?? "" },
  });

  async function onSubmit(values: RecordApplicationNumberFormValues) {
    try {
      await mutation.mutateAsync(values.applicationNumber);
    } catch {
      // A refusal is an outcome, not a crash — the server's reason is rendered
      // from mutation.error below. Once a sequence has been filed under this
      // number the API says so, and that is the message worth showing.
      return;
    }

    onSuccess();
  }

  return (
    <form onSubmit={handleSubmit(onSubmit)} className="space-y-6">
      <FieldGroup>
        <Controller
          control={control}
          name="applicationNumber"
          render={({ field }) => (
            <Field data-invalid={!!errors.applicationNumber}>
              <FieldLabel htmlFor="applicationNumber">
                Application Number
              </FieldLabel>

              <Input id="applicationNumber" {...field} />

              <p className="text-sm text-muted-foreground">
                As the authority assigned it.
              </p>

              <FieldError errors={[errors.applicationNumber]} />
            </Field>
          )}
        />
      </FieldGroup>

      {mutation.isError && (
        <p className="text-sm text-destructive" role="alert">
          {mutation.error.message}
        </p>
      )}

      <div className="flex justify-end gap-2">
        <Button type="submit" disabled={mutation.isPending}>
          {mutation.isPending ? "Saving..." : "Save"}
        </Button>
      </div>
    </form>
  );
}
