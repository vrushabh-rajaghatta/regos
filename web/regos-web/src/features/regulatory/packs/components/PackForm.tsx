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
import { usePharmaceuticalVocabulary } from "@/features/regulatory/medicinalProducts/hooks/usePharmaceuticalVocabulary";

import { useAddPack } from "../hooks/useAddPack";
import { useRestatePack } from "../hooks/useRestatePack";
import type { Pack } from "../types/Pack";
import { packSchema, type PackFormValues } from "../validation/packSchema";

interface PackFormProps {
  medicinalProductId: string;
  /** Set when correcting an existing pack; absent when adding one. */
  pack?: Pack;
  onSuccess(): void;
}

/**
 * Describes a pack, or restates one.
 *
 * **The three facts are settled together** rather than patched one at a time: a
 * corrected size that left the description saying *"carton of 30"* would be a
 * pack contradicting itself.
 *
 * The unit list is the one a presentation and a component already use — a pack
 * of 30 tablets counts the same unit a component measures itself in.
 */
export function PackForm({
  medicinalProductId,
  pack,
  onSuccess,
}: PackFormProps) {
  const { data: vocabulary } = usePharmaceuticalVocabulary();

  const add = useAddPack(medicinalProductId);
  const restate = useRestatePack(medicinalProductId);
  const mutation = pack ? restate : add;

  const {
    control,
    handleSubmit,
    reset,
    formState: { errors },
  } = useForm<PackFormValues>({
    // Required only when adding: correcting a pack must not move its
    // commercial history, so the field is neither shown nor demanded.
    resolver: zodResolver(packSchema(!pack)),
    defaultValues: {
      description: pack?.description ?? "",
      packSizeQuantity: pack?.packSizeQuantity?.toString() ?? "",
      packSizeUnitCode: pack?.packSizeUnitCode ?? "",
      packCode: pack?.packCode ?? "",
      statusDate: "",
    },
  });

  async function onSubmit(values: PackFormValues) {
    const body = {
      description: values.description,
      packSizeQuantity:
        values.packSizeQuantity === "" ? null : Number(values.packSizeQuantity),
      packSizeUnitCode:
        values.packSizeUnitCode === "" ? null : values.packSizeUnitCode,
      packCode: values.packCode === "" ? null : (values.packCode ?? null),
    };

    try {
      if (pack) {
        await restate.mutateAsync({ packagedProductId: pack.id, body });
      } else {
        await add.mutateAsync({ ...body, statusDate: values.statusDate });
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
          name="description"
          render={({ field }) => (
            <Field data-invalid={!!errors.description}>
              <FieldLabel htmlFor="packDescription">Pack</FieldLabel>

              <Input
                id="packDescription"
                placeholder="Carton of 3 blisters × 10 film-coated tablets"
                {...field}
              />

              {errors.description && (
                <FieldError>{errors.description.message}</FieldError>
              )}
            </Field>
          )}
        />

        <Controller
          control={control}
          name="packSizeQuantity"
          render={({ field }) => (
            <Field data-invalid={!!errors.packSizeQuantity}>
              <FieldLabel htmlFor="packSizeQuantity">Contains</FieldLabel>

              <Input
                id="packSizeQuantity"
                type="number"
                step="any"
                placeholder="30"
                {...field}
              />

              {errors.packSizeQuantity && (
                <FieldError>{errors.packSizeQuantity.message}</FieldError>
              )}
            </Field>
          )}
        />

        <Controller
          control={control}
          name="packSizeUnitCode"
          render={({ field }) => (
            <Field data-invalid={!!errors.packSizeUnitCode}>
              <FieldLabel htmlFor="packSizeUnitCode">Of</FieldLabel>

              <select
                id="packSizeUnitCode"
                className="h-9 w-full rounded-md border bg-transparent px-3 text-sm"
                {...field}
              >
                <option value="">Not stated</option>

                {(vocabulary?.unitsOfPresentation ?? []).map((concept) => (
                  <option key={concept.code} value={concept.code}>
                    {concept.display}
                  </option>
                ))}
              </select>

              {errors.packSizeUnitCode && (
                <FieldError>{errors.packSizeUnitCode.message}</FieldError>
              )}
            </Field>
          )}
        />

        <Controller
          control={control}
          name="packCode"
          render={({ field }) => (
            <Field>
              <FieldLabel htmlFor="packCode">Pack code (optional)</FieldLabel>

              <Input id="packCode" placeholder="0123-4567-89" {...field} />

              {/* The market issues it, RegOS does not — so no format is
                  imposed. */}
              <p className="text-xs text-muted-foreground">
                An NDC, a national code, a PZN — whatever this market issues.
              </p>
            </Field>
          )}
        />

        {/* Only when adding: restating what a pack is does not move its
            commercial history, which has its own control. */}
        {!pack && (
          <Controller
            control={control}
            name="statusDate"
            render={({ field }) => (
              <Field data-invalid={!!errors.statusDate}>
                <FieldLabel htmlFor="packStatusDate">Planned since</FieldLabel>

                <Input id="packStatusDate" type="date" {...field} />

                {errors.statusDate && (
                  <FieldError>{errors.statusDate.message}</FieldError>
                )}
              </Field>
            )}
          />
        )}
      </FieldGroup>

      {mutation.isError && (
        <p className="text-sm text-destructive" data-testid="pack-error">
          {(mutation.error as Error).message}
        </p>
      )}

      <div className="flex justify-end">
        <Button type="submit" disabled={mutation.isPending}>
          {mutation.isPending ? "Saving..." : pack ? "Save pack" : "Add pack"}
        </Button>
      </div>
    </form>
  );
}
