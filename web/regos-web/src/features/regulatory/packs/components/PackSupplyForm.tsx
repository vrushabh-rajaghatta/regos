import { zodResolver } from "@hookform/resolvers/zod";
import { Controller, useForm } from "react-hook-form";

import { Button } from "@/components/ui/button";
import {
  Field,
  FieldError,
  FieldGroup,
  FieldLabel,
  FieldTitle,
} from "@/components/ui/field";
import { Input } from "@/components/ui/input";

import { useStatePackSupply } from "../hooks/useStatePackSupply";
import { useSupplyVocabulary } from "../hooks/useSupplyVocabulary";
import type { Pack } from "../types/Pack";
import { NO_SPECIAL_PRECAUTIONS } from "../types/Supply";
import {
  packSupplySchema,
  type PackSupplyFormValues,
} from "../validation/packSupplySchema";

interface PackSupplyFormProps {
  medicinalProductId: string;
  pack: Pack;
  onSuccess(): void;
}

/**
 * How this pack may be handed over, and how long it keeps.
 *
 * **One form over two facts.** Legal status and shelf life move on different
 * clocks — a reclassification is a regulatory decision, a shelf-life extension
 * arrives by variation — and the aggregate keeps them apart for that reason.
 * They share a form because one person states both in one sitting, filling in
 * one section of an SmPC.
 *
 * The period list is the supply vocabulary, never the measurement one: a
 * duration is not a quantity, and offering months beside milligrams is how
 * *"500 months"* becomes a legal strength.
 */
export function PackSupplyForm({
  medicinalProductId,
  pack,
  onSuccess,
}: PackSupplyFormProps) {
  const { data: vocabulary } = useSupplyVocabulary();

  const mutation = useStatePackSupply(medicinalProductId);

  const {
    control,
    handleSubmit,
    formState: { errors },
  } = useForm<PackSupplyFormValues>({
    resolver: zodResolver(packSupplySchema),
    defaultValues: {
      legalStatusOfSupplyCode: pack.legalStatusOfSupplyCode ?? "",
      shelfLifeValue: pack.shelfLifeValue?.toString() ?? "",
      shelfLifeUnitCode: pack.shelfLifeUnitCode ?? "",
      shelfLifeText: pack.shelfLifeText ?? "",
      storageConditionCodes: pack.storageConditions.map((x) => x.code),
    },
  });

  async function onSubmit(values: PackSupplyFormValues) {
    try {
      await mutation.mutateAsync({
        packagedProductId: pack.id,
        body: {
          legalStatusOfSupplyCode:
            values.legalStatusOfSupplyCode === ""
              ? null
              : values.legalStatusOfSupplyCode,
          shelfLifeValue:
            values.shelfLifeValue === "" ? null : Number(values.shelfLifeValue),
          shelfLifeUnitCode:
            values.shelfLifeUnitCode === "" ? null : values.shelfLifeUnitCode,
          shelfLifeText:
            values.shelfLifeText === "" ? null : (values.shelfLifeText ?? null),
          storageConditionCodes: values.storageConditionCodes,
        },
      });
    } catch {
      // A refusal is an outcome, not a crash — the server's reason renders
      // below and the form keeps what was typed.
      return;
    }

    onSuccess();
  }

  return (
    <form onSubmit={handleSubmit(onSubmit)} className="space-y-6">
      <FieldGroup>
        <Controller
          control={control}
          name="legalStatusOfSupplyCode"
          render={({ field }) => (
            <Field>
              <FieldLabel htmlFor="legalStatus">Legal status</FieldLabel>

              <select
                id="legalStatus"
                className="h-9 w-full rounded-md border bg-transparent px-3 text-sm"
                {...field}
              >
                <option value="">Not classified</option>

                {(vocabulary?.legalStatuses ?? []).map((concept) => (
                  <option key={concept.code} value={concept.code}>
                    {concept.display}
                  </option>
                ))}
              </select>

              {/* Why it is on the pack rather than the product. */}
              <p className="text-xs text-muted-foreground">
                A 16-tablet pack may be general sale where a 100-tablet pack of
                the same tablets is pharmacy-only.
              </p>
            </Field>
          )}
        />

        <div className="grid grid-cols-2 gap-3">
          <Controller
            control={control}
            name="shelfLifeValue"
            render={({ field }) => (
              <Field data-invalid={!!errors.shelfLifeValue}>
                <FieldLabel htmlFor="shelfLifeValue">Keeps for</FieldLabel>

                <Input
                  id="shelfLifeValue"
                  type="number"
                  step="any"
                  placeholder="36"
                  {...field}
                />

                {errors.shelfLifeValue && (
                  <FieldError>{errors.shelfLifeValue.message}</FieldError>
                )}
              </Field>
            )}
          />

          <Controller
            control={control}
            name="shelfLifeUnitCode"
            render={({ field }) => (
              <Field data-invalid={!!errors.shelfLifeUnitCode}>
                <FieldLabel htmlFor="shelfLifeUnit">Period</FieldLabel>

                <select
                  id="shelfLifeUnit"
                  className="h-9 w-full rounded-md border bg-transparent px-3 text-sm"
                  {...field}
                >
                  <option value="">Not stated</option>

                  {(vocabulary?.shelfLifePeriods ?? []).map((concept) => (
                    <option key={concept.code} value={concept.code}>
                      {concept.display}
                    </option>
                  ))}
                </select>

                {errors.shelfLifeUnitCode && (
                  <FieldError>{errors.shelfLifeUnitCode.message}</FieldError>
                )}
              </Field>
            )}
          />
        </div>

        {/* Kept literal, and the note says why a user should not convert. */}
        <p className="-mt-3 text-xs text-muted-foreground">
          Recorded as it was approved — 3 years stays 3 years, not 36 months.
        </p>

        <Controller
          control={control}
          name="storageConditionCodes"
          render={({ field }) => (
            <Field data-invalid={!!errors.storageConditionCodes}>
              {/* A title, not a label: each checkbox below carries its own,
                  and a second <label> pointing at nothing would give the group
                  an accessible name that resolves to no control. */}
              <FieldTitle>Storage conditions</FieldTitle>

              <div className="space-y-1" data-testid="storage-conditions">
                {(vocabulary?.storageConditions ?? []).map((concept) => {
                  const checked = field.value.includes(concept.code);

                  return (
                    <div key={concept.code} className="flex items-center gap-2">
                      <input
                        id={`storage-${concept.code}`}
                        type="checkbox"
                        className="size-4"
                        checked={checked}
                        onChange={(event) =>
                          field.onChange(
                            event.target.checked
                              ? [...field.value, concept.code]
                              : field.value.filter(
                                  (code) => code !== concept.code,
                                ),
                          )
                        }
                      />

                      <label
                        htmlFor={`storage-${concept.code}`}
                        className="text-sm"
                      >
                        {concept.display}
                      </label>
                    </div>
                  );
                })}
              </div>

              {errors.storageConditionCodes && (
                <FieldError>{errors.storageConditionCodes.message}</FieldError>
              )}

              {/* The distinction the last entry exists for. */}
              <p className="text-xs text-muted-foreground">
                Leaving these blank means nobody has said yet. "
                {vocabulary?.storageConditions.find(
                  (x) => x.code === NO_SPECIAL_PRECAUTIONS,
                )?.display ?? "No special storage precautions"}
                " means somebody checked.
              </p>
            </Field>
          )}
        />

        <Controller
          control={control}
          name="shelfLifeText"
          render={({ field }) => (
            <Field>
              <FieldLabel htmlFor="shelfLifeText">
                Wording on the label (optional)
              </FieldLabel>

              <Input
                id="shelfLifeText"
                placeholder="After first opening: use within 28 days."
                {...field}
              />
            </Field>
          )}
        />
      </FieldGroup>

      {mutation.isError && (
        <p className="text-sm text-destructive" data-testid="pack-supply-error">
          {(mutation.error as Error).message}
        </p>
      )}

      <div className="flex justify-end">
        <Button type="submit" disabled={mutation.isPending}>
          {mutation.isPending ? "Saving..." : "Save supply"}
        </Button>
      </div>
    </form>
  );
}
