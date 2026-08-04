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

import { useRecordAtcCode } from "../hooks/useRecordAtcCode";
import {
  atcCodeSchema,
  type AtcCodeFormValues,
} from "../validation/atcCodeSchema";

interface AtcCodeFormProps {
  medicinalProductId: string;
  currentAtcCode: string | null;
  onSuccess(): void;
}

/**
 * One field, and a sentence saying what RegOS is and is not claiming about it.
 *
 * The platform holds no WHO ATC licence, so it checks the code's *shape* and
 * cannot check that the code exists. Saying so here is the same honesty the
 * seed file carries — a user should not read acceptance as verification.
 */
export function AtcCodeForm({
  medicinalProductId,
  currentAtcCode,
  onSuccess,
}: AtcCodeFormProps) {
  const mutation = useRecordAtcCode(medicinalProductId);

  const {
    control,
    handleSubmit,
    formState: { errors },
  } = useForm<AtcCodeFormValues>({
    resolver: zodResolver(atcCodeSchema),
    defaultValues: { atcCode: currentAtcCode ?? "" },
  });

  async function onSubmit(values: AtcCodeFormValues) {
    try {
      // Blank clears it. Absence is an ordinary state for a market, so this is
      // a correction rather than a separate action.
      await mutation.mutateAsync(values.atcCode?.trim() || null);
    } catch {
      // A refusal is an outcome, not a crash — the server's reason renders
      // below and says what an ATC code looks like.
      return;
    }

    onSuccess();
  }

  return (
    <form onSubmit={handleSubmit(onSubmit)} className="space-y-6">
      <FieldGroup>
        <Controller
          control={control}
          name="atcCode"
          render={({ field }) => (
            <Field data-invalid={!!errors.atcCode}>
              <FieldLabel htmlFor="atc-code">ATC code</FieldLabel>

              <Input id="atc-code" placeholder="e.g. N02BE01" {...field} />

              <p className="text-xs text-muted-foreground">
                RegOS checks the shape only — it does not hold the WHO ATC
                index, so the code is recorded as you give it. Leave blank to
                clear.
              </p>

              <FieldError errors={[errors.atcCode]} />
            </Field>
          )}
        />
      </FieldGroup>

      {mutation.isError && (
        <p className="text-sm text-destructive" data-testid="atc-code-error">
          {(mutation.error as Error).message}
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
