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
import { useSubstances } from "@/features/regulatory/substances/hooks/useSubstances";

import { useClinicalVocabulary } from "../hooks/useClinicalVocabulary";
import { useRecordInteraction } from "../hooks/useRecordInteraction";
import { chosen, NONE } from "../validation/populationSchema";
import {
  recordInteractionSchema,
  type RecordInteractionFormValues,
} from "../validation/recordInteractionSchema";

interface RecordInteractionFormProps {
  medicinalProductId: string;
  onSuccess(): void;
}

/**
 * What this product clashes with.
 *
 * **The interactant is free text with an optional catalogue link beside it.**
 * Most interactants are not substances RegOS knows — grapefruit juice, alcohol,
 * "CYP3A4 inhibitors" — so the text is required and the link is not. Setting the
 * link is what turns *"which of our products interact with warfarin?"* into a
 * join rather than a string match.
 */
export function RecordInteractionForm({
  medicinalProductId,
  onSuccess,
}: RecordInteractionFormProps) {
  const mutation = useRecordInteraction(medicinalProductId);
  const { data: vocabulary, isLoading } = useClinicalVocabulary();
  const { data: substances } = useSubstances();

  const {
    control,
    handleSubmit,
    reset,
    formState: { errors },
  } = useForm<RecordInteractionFormValues>({
    resolver: zodResolver(recordInteractionSchema),
    defaultValues: {
      interactionTypeCode: "DRUG-DRUG",
      labelText: "",
      interactant: "",
      interactantSubstanceId: NONE,
      management: "",
      severityCode: NONE,
    },
  });

  async function onSubmit(values: RecordInteractionFormValues) {
    try {
      await mutation.mutateAsync({
        interactionTypeCode: values.interactionTypeCode,
        labelText: values.labelText,
        interactant: values.interactant,
        interactantSubstanceId: chosen(values.interactantSubstanceId),
        management: values.management?.trim() || null,
        severityCode: chosen(values.severityCode),
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
          name="interactionTypeCode"
          render={({ field }) => (
            <Field data-invalid={!!errors.interactionTypeCode}>
              <FieldLabel htmlFor="interactionTypeCode">Kind</FieldLabel>

              <Select
                onValueChange={field.onChange}
                value={field.value}
                disabled={isLoading}
              >
                <SelectTrigger id="interactionTypeCode">
                  <SelectValue placeholder="Select a kind" />
                </SelectTrigger>

                <SelectContent>
                  {(vocabulary?.interactionTypes ?? []).map((concept) => (
                    <SelectItem key={concept.code} value={concept.code}>
                      {concept.display}
                    </SelectItem>
                  ))}
                </SelectContent>
              </Select>

              <FieldError errors={[errors.interactionTypeCode]} />
            </Field>
          )}
        />

        <Controller
          control={control}
          name="interactant"
          render={({ field }) => (
            <Field data-invalid={!!errors.interactant}>
              <FieldLabel htmlFor="interactant">Interacts with</FieldLabel>

              <Input
                id="interactant"
                placeholder="e.g. warfarin, grapefruit juice, CYP3A4 inhibitors"
                {...field}
              />

              <FieldError errors={[errors.interactant]} />
            </Field>
          )}
        />

        <Controller
          control={control}
          name="interactantSubstanceId"
          render={({ field }) => (
            <Field>
              <FieldLabel htmlFor="interactantSubstanceId">
                Link to a substance
              </FieldLabel>

              <Select onValueChange={field.onChange} value={field.value}>
                <SelectTrigger id="interactantSubstanceId">
                  <SelectValue placeholder="Not linked" />
                </SelectTrigger>

                <SelectContent>
                  <SelectItem value={NONE}>Not linked</SelectItem>
                  {(substances ?? []).map((substance) => (
                    <SelectItem key={substance.id} value={substance.id}>
                      {substance.name}
                    </SelectItem>
                  ))}
                </SelectContent>
              </Select>

              <p className="text-xs text-muted-foreground">
                Optional. Most interactants are not compounds in the catalogue —
                linking one makes the interaction findable from the substance.
              </p>
            </Field>
          )}
        />

        <Controller
          control={control}
          name="labelText"
          render={({ field }) => (
            <Field data-invalid={!!errors.labelText}>
              <FieldLabel htmlFor="labelText">What happens</FieldLabel>
              <Input id="labelText" {...field} />
              <FieldError errors={[errors.labelText]} />
            </Field>
          )}
        />

        <Controller
          control={control}
          name="management"
          render={({ field }) => (
            <Field data-invalid={!!errors.management}>
              <FieldLabel htmlFor="management">What to do</FieldLabel>
              <Input id="management" placeholder="Optional" {...field} />
              <FieldError errors={[errors.management]} />
            </Field>
          )}
        />

        <Controller
          control={control}
          name="severityCode"
          render={({ field }) => (
            <Field>
              <FieldLabel htmlFor="severityCode">Severity</FieldLabel>

              <Select
                onValueChange={field.onChange}
                value={field.value}
                disabled={isLoading}
              >
                <SelectTrigger id="severityCode">
                  <SelectValue placeholder="Not graded" />
                </SelectTrigger>

                <SelectContent>
                  <SelectItem value={NONE}>Not graded</SelectItem>
                  {(vocabulary?.interactionSeverities ?? []).map((concept) => (
                    <SelectItem key={concept.code} value={concept.code}>
                      {concept.display}
                    </SelectItem>
                  ))}
                </SelectContent>
              </Select>

              <p className="text-xs text-muted-foreground">
                Optional — many labels describe an interaction without grading
                it.
              </p>
            </Field>
          )}
        />
      </FieldGroup>

      {mutation.isError && (
        <p
          className="text-sm text-destructive"
          data-testid="record-interaction-error"
        >
          {(mutation.error as Error).message}
        </p>
      )}

      <div className="flex justify-end gap-2">
        <Button type="submit" disabled={mutation.isPending}>
          {mutation.isPending ? "Recording..." : "Record interaction"}
        </Button>
      </div>
    </form>
  );
}
