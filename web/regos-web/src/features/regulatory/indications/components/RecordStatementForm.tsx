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
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@/components/ui/select";

import { useClinicalVocabulary } from "../hooks/useClinicalVocabulary";
import { useRecordStatement } from "../hooks/useRecordStatement";
import type { StatementKind } from "../types/StatementKind";
import { chosen, NONE } from "../validation/populationSchema";
import {
  recordStatementSchema,
  type RecordStatementFormValues,
} from "../validation/recordStatementSchema";

interface RecordStatementFormProps {
  kind: Exclude<StatementKind, "indications">;
  medicinalProductId: string;
  onSuccess(): void;
}

/**
 * Records a statement inside this market's approved label.
 *
 * **No approval date**, unlike an indication's form. That difference is the
 * design: an indication is an authorisation the authority grants, so it carries
 * the date it was granted; a contraindication is content within a label, and
 * what changes it is a new label revision.
 */
export function RecordStatementForm({
  kind,
  medicinalProductId,
  onSuccess,
}: RecordStatementFormProps) {
  const mutation = useRecordStatement(kind, medicinalProductId);
  const { data: vocabulary, isLoading } = useClinicalVocabulary();

  const isEffect = kind === "undesirable-effects";

  const {
    control,
    handleSubmit,
    reset,
    formState: { errors },
  } = useForm<RecordStatementFormValues>({
    resolver: zodResolver(recordStatementSchema),
    defaultValues: { conditionCode: "", labelText: "", frequencyCode: NONE },
  });

  async function onSubmit(values: RecordStatementFormValues) {
    try {
      await mutation.mutateAsync({
        conditionCode: values.conditionCode,
        labelText: values.labelText,
        frequencyCode: isEffect ? chosen(values.frequencyCode) : null,
      });
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
          name="conditionCode"
          render={({ field }) => (
            <Field data-invalid={!!errors.conditionCode}>
              <FieldLabel htmlFor="conditionCode">
                {isEffect ? "Effect" : "Condition"}
              </FieldLabel>

              <Select
                onValueChange={field.onChange}
                value={field.value}
                disabled={isLoading}
              >
                <SelectTrigger id="conditionCode">
                  <SelectValue
                    placeholder={isEffect ? "Select an effect" : "Select a condition"}
                  />
                </SelectTrigger>

                <SelectContent>
                  {(vocabulary?.conditions ?? []).map((concept) => (
                    <SelectItem key={concept.code} value={concept.code}>
                      {concept.display}
                    </SelectItem>
                  ))}
                </SelectContent>
              </Select>

              <FieldError errors={[errors.conditionCode]} />
            </Field>
          )}
        />

        <Controller
          control={control}
          name="labelText"
          render={({ field }) => (
            <Field data-invalid={!!errors.labelText}>
              <FieldLabel htmlFor="labelText">As the label says it</FieldLabel>
              <Input id="labelText" {...field} />
              <FieldError errors={[errors.labelText]} />
            </Field>
          )}
        />

        {/* The one field the three statement types do not share. */}
        {isEffect && (
          <Controller
            control={control}
            name="frequencyCode"
            render={({ field }) => (
              <Field>
                <FieldLabel htmlFor="frequencyCode">How often</FieldLabel>

                <Select
                  onValueChange={field.onChange}
                  value={field.value}
                  disabled={isLoading}
                >
                  <SelectTrigger id="frequencyCode">
                    <SelectValue placeholder="Not stated" />
                  </SelectTrigger>

                  <SelectContent>
                    <SelectItem value={NONE}>Not stated</SelectItem>
                    {(vocabulary?.frequencies ?? []).map((concept) => (
                      <SelectItem key={concept.code} value={concept.code}>
                        {concept.display}
                      </SelectItem>
                    ))}
                  </SelectContent>
                </Select>

                <p className="text-xs text-muted-foreground">
                  Recorded as the label states it, never computed — the bands
                  rest on trial data RegOS does not hold.
                </p>
              </Field>
            )}
          />
        )}
      </FieldGroup>

      {mutation.isError && (
        <p
          className="text-sm text-destructive"
          data-testid="record-statement-error"
        >
          {(mutation.error as Error).message}
        </p>
      )}

      <div className="flex justify-end gap-2">
        <Button type="submit" disabled={mutation.isPending}>
          {mutation.isPending ? "Recording..." : "Record"}
        </Button>
      </div>
    </form>
  );
}
