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
import { useRecordIndication } from "../hooks/useRecordIndication";
import {
  recordIndicationSchema,
  type RecordIndicationFormValues,
} from "../validation/recordIndicationSchema";

interface RecordIndicationFormProps {
  medicinalProductId: string;
  onSuccess(): void;
}

/**
 * Records what an authority approved this product to treat.
 *
 * **Two fields for one fact, on purpose.** The condition is a code, so the same
 * authorisation is recognisable in Japan and France; the text is what this
 * market's label actually says. Free text alone could not be asked backwards.
 */
export function RecordIndicationForm({
  medicinalProductId,
  onSuccess,
}: RecordIndicationFormProps) {
  const mutation = useRecordIndication(medicinalProductId);
  const { data: vocabulary, isLoading } = useClinicalVocabulary();

  const {
    control,
    handleSubmit,
    reset,
    formState: { errors },
  } = useForm<RecordIndicationFormValues>({
    resolver: zodResolver(recordIndicationSchema),
    defaultValues: { conditionCode: "", labelText: "", approvedOn: "" },
  });

  async function onSubmit(values: RecordIndicationFormValues) {
    try {
      await mutation.mutateAsync(values);
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
              <FieldLabel htmlFor="conditionCode">Condition</FieldLabel>

              <Select
                onValueChange={field.onChange}
                value={field.value}
                disabled={isLoading}
              >
                <SelectTrigger id="conditionCode">
                  <SelectValue placeholder="Select a condition" />
                </SelectTrigger>

                <SelectContent>
                  {(vocabulary?.conditions ?? []).map((concept) => (
                    <SelectItem key={concept.code} value={concept.code}>
                      {concept.display}
                    </SelectItem>
                  ))}
                </SelectContent>
              </Select>

              <p className="text-xs text-muted-foreground">
                A small demonstration set — not MedDRA, SNOMED or ICD. The code
                is what makes this indication comparable across markets.
              </p>

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

              <Input
                id="labelText"
                placeholder="e.g. Treatment of type 2 diabetes mellitus in adults."
                {...field}
              />

              <FieldError errors={[errors.labelText]} />
            </Field>
          )}
        />

        <Controller
          control={control}
          name="approvedOn"
          render={({ field }) => (
            <Field data-invalid={!!errors.approvedOn}>
              <FieldLabel htmlFor="approvedOn">Approved on</FieldLabel>

              <Input id="approvedOn" type="date" {...field} />

              <FieldError errors={[errors.approvedOn]} />
            </Field>
          )}
        />
      </FieldGroup>

      {mutation.isError && (
        <p
          className="text-sm text-destructive"
          data-testid="record-indication-error"
        >
          {(mutation.error as Error).message}
        </p>
      )}

      <div className="flex justify-end gap-2">
        <Button type="submit" disabled={mutation.isPending}>
          {mutation.isPending ? "Recording..." : "Record indication"}
        </Button>
      </div>
    </form>
  );
}
