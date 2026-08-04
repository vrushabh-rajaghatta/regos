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
import { useSaveStatementPopulation } from "../hooks/useSaveStatementPopulation";
import type { Population } from "../types/Indication";
import type { StatementKind } from "../types/StatementKind";
import {
  chosen,
  NONE,
  populationSchema,
  type PopulationFormValues,
} from "../validation/populationSchema";

interface PopulationFormProps {
  /** Which statement owns it — the same form serves all three. */
  kind: StatementKind;
  statementId: string;
  /** Present when correcting an existing qualifier in place. */
  population?: Population;
  onSuccess(): void;
}

/**
 * Who the statement applies to.
 *
 * **Add and amend are the same form**, and the difference is one id: amending
 * PUTs onto the population's own route, so the qualifier keeps its identity
 * through a correction rather than being replaced by a new one (EPIC-018 D2).
 */
export function PopulationForm({
  kind,
  statementId,
  population,
  onSuccess,
}: PopulationFormProps) {
  const mutation = useSaveStatementPopulation();
  const { data: vocabulary, isLoading } = useClinicalVocabulary();

  const {
    control,
    handleSubmit,
    formState: { errors },
  } = useForm<PopulationFormValues>({
    resolver: zodResolver(populationSchema),
    defaultValues: {
      ageLow: population?.ageLow?.toString() ?? "",
      ageHigh: population?.ageHigh?.toString() ?? "",
      ageUnitCode: population?.ageUnitCode ?? NONE,
      genderCode: population?.genderCode ?? "ALL",
      physiologicalConditionCode:
        population?.physiologicalConditionCode ?? NONE,
      description: population?.description ?? "",
    },
  });

  async function onSubmit(values: PopulationFormValues) {
    try {
      await mutation.mutateAsync({
        kind,
        statementId,
        populationId: population?.id ?? null,
        body: {
          ageLow: values.ageLow?.trim() ? Number(values.ageLow) : null,
          ageHigh: values.ageHigh?.trim() ? Number(values.ageHigh) : null,
          ageUnitCode: chosen(values.ageUnitCode),
          genderCode: values.genderCode,
          physiologicalConditionCode: chosen(
            values.physiologicalConditionCode,
          ),
          description: values.description?.trim() || null,
        },
      });
    } catch {
      // The server refuses an age with no unit, a unit with no age, and an
      // inverted range. Each says why, and each renders below.
      return;
    }

    onSuccess();
  }

  return (
    <form onSubmit={handleSubmit(onSubmit)} className="space-y-6">
      <FieldGroup>
        <div className="grid grid-cols-3 gap-2">
          <Controller
            control={control}
            name="ageLow"
            render={({ field }) => (
              <Field data-invalid={!!errors.ageLow}>
                <FieldLabel htmlFor="ageLow">From age</FieldLabel>
                <Input id="ageLow" inputMode="numeric" {...field} />
                <FieldError errors={[errors.ageLow]} />
              </Field>
            )}
          />

          <Controller
            control={control}
            name="ageHigh"
            render={({ field }) => (
              <Field data-invalid={!!errors.ageHigh}>
                <FieldLabel htmlFor="ageHigh">To age</FieldLabel>
                <Input id="ageHigh" inputMode="numeric" {...field} />
                <FieldError errors={[errors.ageHigh]} />
              </Field>
            )}
          />

          <Controller
            control={control}
            name="ageUnitCode"
            render={({ field }) => (
              <Field>
                <FieldLabel htmlFor="ageUnitCode">Unit</FieldLabel>

                <Select
                  onValueChange={field.onChange}
                  value={field.value}
                  disabled={isLoading}
                >
                  <SelectTrigger id="ageUnitCode">
                    <SelectValue placeholder="None" />
                  </SelectTrigger>

                  <SelectContent>
                    <SelectItem value={NONE}>None</SelectItem>
                    {(vocabulary?.ageUnits ?? []).map((concept) => (
                      <SelectItem key={concept.code} value={concept.code}>
                        {concept.display}
                      </SelectItem>
                    ))}
                  </SelectContent>
                </Select>
              </Field>
            )}
          />
        </div>

        <Controller
          control={control}
          name="genderCode"
          render={({ field }) => (
            <Field data-invalid={!!errors.genderCode}>
              <FieldLabel htmlFor="genderCode">Applies to</FieldLabel>

              <Select
                onValueChange={field.onChange}
                value={field.value}
                disabled={isLoading}
              >
                <SelectTrigger id="genderCode">
                  <SelectValue placeholder="Any" />
                </SelectTrigger>

                <SelectContent>
                  {(vocabulary?.genders ?? []).map((concept) => (
                    <SelectItem key={concept.code} value={concept.code}>
                      {concept.display}
                    </SelectItem>
                  ))}
                </SelectContent>
              </Select>

              <FieldError errors={[errors.genderCode]} />
            </Field>
          )}
        />

        <Controller
          control={control}
          name="physiologicalConditionCode"
          render={({ field }) => (
            <Field>
              <FieldLabel htmlFor="physiologicalConditionCode">
                Physiological condition
              </FieldLabel>

              <Select
                onValueChange={field.onChange}
                value={field.value}
                disabled={isLoading}
              >
                <SelectTrigger id="physiologicalConditionCode">
                  <SelectValue placeholder="None" />
                </SelectTrigger>

                <SelectContent>
                  <SelectItem value={NONE}>None</SelectItem>
                  {(vocabulary?.physiologicalConditions ?? []).map((concept) => (
                    <SelectItem key={concept.code} value={concept.code}>
                      {concept.display}
                    </SelectItem>
                  ))}
                </SelectContent>
              </Select>
            </Field>
          )}
        />

        <Controller
          control={control}
          name="description"
          render={({ field }) => (
            <Field data-invalid={!!errors.description}>
              <FieldLabel htmlFor="description">In the label's words</FieldLabel>
              <Input id="description" placeholder="Optional" {...field} />
              <FieldError errors={[errors.description]} />
            </Field>
          )}
        />
      </FieldGroup>

      {mutation.isError && (
        <p className="text-sm text-destructive" data-testid="population-error">
          {(mutation.error as Error).message}
        </p>
      )}

      <div className="flex justify-end gap-2">
        <Button type="submit" disabled={mutation.isPending}>
          {population ? "Save correction" : "Add population"}
        </Button>
      </div>
    </form>
  );
}
