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

import { useSiteDirectory } from "../../organizations/hooks/useSiteDirectory";
import { useSubstances } from "../../substances/hooks/useSubstances";
import { useAddIngredient } from "../hooks/useAddIngredient";
import { useMeasurementUnits } from "../hooks/useMeasurementUnits";
import { useRestateIngredient } from "../hooks/useRestateIngredient";
import type { Ingredient } from "../types/Presentation";
import {
  ingredientSchema,
  NO_UNIT,
  type IngredientFormValues,
} from "../validation/ingredientSchema";

interface IngredientFormProps {
  medicinalProductId: string;
  presentationId: string;
  /** Present when correcting one, absent when adding. */
  ingredient?: Ingredient;
  onSuccess(): void;
}

/**
 * A substance, the role it plays, and how much of it there is.
 *
 * **The substance is chosen once and never changed.** A different substance is
 * a different ingredient, so swapping one is add-then-remove — which is also
 * why the picker is disabled when correcting.
 *
 * **The denominator is the concentration case.** Leaving it blank gives a point
 * strength — *500 mg* — which in a presentation whose dose form is *Tablet*
 * already means 500 mg per tablet. The units offered here measure quantity and
 * never name an article, so a strength cannot repeat what the presentation
 * says.
 */
export function IngredientForm({
  medicinalProductId,
  presentationId,
  ingredient,
  onSuccess,
}: IngredientFormProps) {
  const add = useAddIngredient(medicinalProductId, presentationId);
  const restate = useRestateIngredient(medicinalProductId, presentationId);

  const mutation = ingredient ? restate : add;

  // A plain list, not a search: the catalogue is six shared compounds plus the
  // organisation's own. It becomes a search when licensed terminology arrives
  // and the list stops fitting on a screen.
  const { data: substances, isLoading: loadingSubstances } = useSubstances();
  const { data: sites } = useSiteDirectory();
  const { data: units, isLoading: loadingUnits } = useMeasurementUnits();

  const {
    control,
    handleSubmit,
    reset,
    formState: { errors },
  } = useForm<IngredientFormValues>({
    resolver: zodResolver(ingredientSchema),
    defaultValues: {
      substanceId: ingredient?.substanceId ?? "",
      role: ingredient?.role ?? "Active",
      numeratorValue: ingredient?.strength
        ? String(ingredient.strength.numeratorValue)
        : "",
      numeratorUnitCode: ingredient?.strength?.numeratorUnit.code ?? NO_UNIT,
      denominatorValue: ingredient?.strength?.denominatorValue
        ? String(ingredient.strength.denominatorValue)
        : "",
      denominatorUnitCode:
        ingredient?.strength?.denominatorUnit?.code ?? NO_UNIT,
      manufacturingSourceSiteId: ingredient?.manufacturingSourceSiteId ?? "",
    },
  });

  async function onSubmit(values: IngredientFormValues) {
    const unit = (code?: string) => (code && code !== NO_UNIT ? code : null);
    const number = (value?: string) =>
      value && value !== "" ? Number(value) : null;

    const body = {
      role: values.role,
      numeratorValue: number(values.numeratorValue),
      numeratorUnitCode: unit(values.numeratorUnitCode),
      denominatorValue: number(values.denominatorValue),
      denominatorUnitCode: unit(values.denominatorUnitCode),

      // Sent on restate as well as add, and that is the point: the aggregate
      // takes no default here, so an omitted value would erase provenance
      // rather than leave it alone.
      manufacturingSourceSiteId:
        values.manufacturingSourceSiteId === ""
          ? null
          : (values.manufacturingSourceSiteId ?? null),
    };

    try {
      if (ingredient) {
        await restate.mutateAsync({
          ...body,
          ingredientId: ingredient.ingredientId,
        });
      } else {
        await add.mutateAsync({ ...body, substanceId: values.substanceId });
      }
    } catch {
      // A refusal is an outcome, not a crash — the server's reason renders
      // below and the form keeps what was typed.
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
          name="substanceId"
          render={({ field }) => (
            <Field data-invalid={!!errors.substanceId}>
              <FieldLabel htmlFor="substance">Substance</FieldLabel>

              <Select
                onValueChange={field.onChange}
                value={field.value}
                disabled={loadingSubstances || ingredient !== undefined}
              >
                <SelectTrigger id="substance">
                  <SelectValue placeholder="Select a substance" />
                </SelectTrigger>

                <SelectContent>
                  {(substances ?? []).map((substance) => (
                    <SelectItem key={substance.id} value={substance.id}>
                      {substance.name}
                      {substance.isShared ? "" : " (ours)"}
                    </SelectItem>
                  ))}
                </SelectContent>
              </Select>

              {ingredient && (
                <p className="text-xs text-muted-foreground">
                  A different substance is a different ingredient — add the new
                  one, then remove this.
                </p>
              )}

              <FieldError errors={[errors.substanceId]} />
            </Field>
          )}
        />

        <Controller
          control={control}
          name="role"
          render={({ field }) => (
            <Field data-invalid={!!errors.role}>
              <FieldLabel htmlFor="role">Role</FieldLabel>

              <Select onValueChange={field.onChange} value={field.value}>
                <SelectTrigger id="role">
                  <SelectValue placeholder="Select a role" />
                </SelectTrigger>

                <SelectContent>
                  <SelectItem value="Active">
                    Active — what the product works by
                  </SelectItem>
                  <SelectItem value="Excipient">
                    Excipient — everything else in the formulation
                  </SelectItem>
                </SelectContent>
              </Select>

              <FieldError errors={[errors.role]} />
            </Field>
          )}
        />

        <div className="grid grid-cols-2 gap-3">
          <Controller
            control={control}
            name="numeratorValue"
            render={({ field }) => (
              <Field data-invalid={!!errors.numeratorValue}>
                <FieldLabel htmlFor="numerator-value">Strength</FieldLabel>

                <Input
                  id="numerator-value"
                  inputMode="decimal"
                  placeholder="e.g. 500"
                  {...field}
                />

                <FieldError errors={[errors.numeratorValue]} />
              </Field>
            )}
          />

          <Controller
            control={control}
            name="numeratorUnitCode"
            render={({ field }) => (
              <Field data-invalid={!!errors.numeratorUnitCode}>
                <FieldLabel htmlFor="numerator-unit">Unit</FieldLabel>

                <Select
                  onValueChange={field.onChange}
                  value={field.value}
                  disabled={loadingUnits}
                >
                  <SelectTrigger id="numerator-unit">
                    <SelectValue placeholder="Unit" />
                  </SelectTrigger>

                  <SelectContent>
                    <SelectItem value={NO_UNIT}>None</SelectItem>

                    {(units ?? []).map((unit) => (
                      <SelectItem key={unit.code} value={unit.code}>
                        {unit.display}
                      </SelectItem>
                    ))}
                  </SelectContent>
                </Select>

                <FieldError errors={[errors.numeratorUnitCode]} />
              </Field>
            )}
          />
        </div>

        <div className="grid grid-cols-2 gap-3">
          <Controller
            control={control}
            name="denominatorValue"
            render={({ field }) => (
              <Field data-invalid={!!errors.denominatorValue}>
                <FieldLabel htmlFor="denominator-value">Per</FieldLabel>

                <Input
                  id="denominator-value"
                  inputMode="decimal"
                  placeholder="Leave blank"
                  {...field}
                />

                <FieldError errors={[errors.denominatorValue]} />
              </Field>
            )}
          />

          <Controller
            control={control}
            name="denominatorUnitCode"
            render={({ field }) => (
              <Field data-invalid={!!errors.denominatorUnitCode}>
                <FieldLabel htmlFor="denominator-unit">Per unit</FieldLabel>

                <Select
                  onValueChange={field.onChange}
                  value={field.value}
                  disabled={loadingUnits}
                >
                  <SelectTrigger id="denominator-unit">
                    <SelectValue placeholder="None" />
                  </SelectTrigger>

                  <SelectContent>
                    <SelectItem value={NO_UNIT}>None</SelectItem>

                    {(units ?? []).map((unit) => (
                      <SelectItem key={unit.code} value={unit.code}>
                        {unit.display}
                      </SelectItem>
                    ))}
                  </SelectContent>
                </Select>

                <FieldError errors={[errors.denominatorUnitCode]} />
              </Field>
            )}
          />
        </div>

        <p className="text-xs text-muted-foreground">
          Leave <em>Per</em> blank for a strength like 500 mg — the presentation
          already says what it comes in. Fill it in for a concentration, like 10
          mg per 1 mL.
        </p>

        <Controller
          control={control}
          name="manufacturingSourceSiteId"
          render={({ field }) => (
            <Field>
              <FieldLabel htmlFor="ingredient-source">
                Sourced from (optional)
              </FieldLabel>

              {/* A native select rather than the Radix one the units use: the
                  empty option is a real choice here — "nobody has said" — and
                  Radix refuses an empty item value, which is the whole reason
                  NO_UNIT exists one field up. */}
              <select
                id="ingredient-source"
                className="h-9 w-full rounded-md border bg-transparent px-3 text-sm"
                value={field.value ?? ""}
                onChange={field.onChange}
                onBlur={field.onBlur}
                name={field.name}
              >
                <option value="">Not stated</option>

                {(sites ?? []).map((site) => (
                  <option key={site.siteId} value={site.siteId}>
                    {site.name} — {site.countryName}
                  </option>
                ))}
              </select>

              {/* Why this is not the same question the market page asks. */}
              <p className="text-xs text-muted-foreground">
                Where this substance comes from — not where the finished product
                is made. A product made at one site routinely takes its actives
                from several others.
              </p>
            </Field>
          )}
        />
      </FieldGroup>

      {mutation.isError && (
        <p className="text-sm text-destructive" data-testid="ingredient-error">
          {(mutation.error as Error).message}
        </p>
      )}

      <div className="flex justify-end gap-2">
        <Button type="submit" disabled={mutation.isPending}>
          {mutation.isPending
            ? "Saving..."
            : ingredient
              ? "Save ingredient"
              : "Add ingredient"}
        </Button>
      </div>
    </form>
  );
}
