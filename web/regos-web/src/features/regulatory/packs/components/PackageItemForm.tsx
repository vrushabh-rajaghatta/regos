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

import { usePackagingVocabulary } from "../hooks/usePackagingVocabulary";
import { useSavePackageItem } from "../hooks/useSavePackageItem";
import type { PackageItem } from "../types/PackageItem";
import {
  packageItemSchema,
  type PackageItemFormValues,
} from "../validation/packageItemSchema";

interface PackageItemFormProps {
  packagedProductId: string;
  /** Set when correcting a layer; absent when adding one. */
  item?: PackageItem;
  /** Where a new layer goes. Ignored when correcting. */
  parentPackageItemId: string | null;
  onSuccess(): void;
}

/**
 * Describes a layer of the pack, or restates one.
 *
 * **Where it sits is not here.** Moving a layer is a statement about the whole
 * tree, checked against every other layer, and it has its own control — folding
 * it into an edit would hide that (ADR-061 §2).
 *
 * The unit list is the presentation vocabulary, not a second copy: a layer's
 * quantity counts the same units a presentation does.
 */
export function PackageItemForm({
  packagedProductId,
  item,
  parentPackageItemId,
  onSuccess,
}: PackageItemFormProps) {
  const { data: packaging } = usePackagingVocabulary();
  const { data: pharmaceutical } = usePharmaceuticalVocabulary();

  const mutation = useSavePackageItem(packagedProductId);

  const {
    control,
    handleSubmit,
    reset,
    formState: { errors },
  } = useForm<PackageItemFormValues>({
    resolver: zodResolver(packageItemSchema),
    defaultValues: {
      itemTypeCode: item?.itemTypeCode ?? "",
      materialCode: item?.materialCode ?? "",
      quantity: item?.quantity?.toString() ?? "1",
      unitOfPresentationCode: item?.unitOfPresentationCode ?? "",
      description: item?.description ?? "",
    },
  });

  async function onSubmit(values: PackageItemFormValues) {
    try {
      await mutation.mutateAsync({
        packageItemId: item?.id,
        body: {
          // Only sent when adding: restating never moves a layer.
          ...(item ? {} : { parentPackageItemId }),
          itemTypeCode: values.itemTypeCode,
          materialCode: values.materialCode === "" ? null : values.materialCode,
          quantity: Number(values.quantity),
          unitOfPresentationCode:
            values.unitOfPresentationCode === ""
              ? null
              : values.unitOfPresentationCode,
          description:
            values.description === "" ? null : (values.description ?? null),
        },
      });
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
          name="itemTypeCode"
          render={({ field }) => (
            <Field data-invalid={!!errors.itemTypeCode}>
              <FieldLabel htmlFor="itemTypeCode">Layer</FieldLabel>

              <select
                id="itemTypeCode"
                className="h-9 w-full rounded-md border bg-transparent px-3 text-sm"
                {...field}
              >
                <option value="">Choose one</option>

                {(packaging?.packageItemTypes ?? []).map((concept) => (
                  <option key={concept.code} value={concept.code}>
                    {concept.display}
                  </option>
                ))}
              </select>

              {errors.itemTypeCode && (
                <FieldError>{errors.itemTypeCode.message}</FieldError>
              )}
            </Field>
          )}
        />

        <Controller
          control={control}
          name="quantity"
          render={({ field }) => (
            <Field data-invalid={!!errors.quantity}>
              <FieldLabel htmlFor="itemQuantity">How many</FieldLabel>

              <Input id="itemQuantity" type="number" step="any" {...field} />

              {errors.quantity && (
                <FieldError>{errors.quantity.message}</FieldError>
              )}
            </Field>
          )}
        />

        <Controller
          control={control}
          name="unitOfPresentationCode"
          render={({ field }) => (
            <Field>
              <FieldLabel htmlFor="itemUnit">Counted in (optional)</FieldLabel>

              <select
                id="itemUnit"
                className="h-9 w-full rounded-md border bg-transparent px-3 text-sm"
                {...field}
              >
                <option value="">Not stated</option>

                {(pharmaceutical?.unitsOfPresentation ?? []).map((concept) => (
                  <option key={concept.code} value={concept.code}>
                    {concept.display}
                  </option>
                ))}
              </select>
            </Field>
          )}
        />

        <Controller
          control={control}
          name="materialCode"
          render={({ field }) => (
            <Field>
              <FieldLabel htmlFor="itemMaterial">
                Made of (optional)
              </FieldLabel>

              <select
                id="itemMaterial"
                className="h-9 w-full rounded-md border bg-transparent px-3 text-sm"
                {...field}
              >
                <option value="">Not stated</option>

                {(packaging?.materials ?? []).map((concept) => (
                  <option key={concept.code} value={concept.code}>
                    {concept.display}
                  </option>
                ))}
              </select>

              {/* Why material lives here and nowhere near a component. */}
              <p className="text-xs text-muted-foreground">
                A blister's laminate is what the stability data was generated
                against; an outer carton's board grade rarely is.
              </p>
            </Field>
          )}
        />

        <Controller
          control={control}
          name="description"
          render={({ field }) => (
            <Field>
              <FieldLabel htmlFor="itemDescription">
                Anything else (optional)
              </FieldLabel>

              <Input
                id="itemDescription"
                placeholder="Child-resistant closure."
                {...field}
              />
            </Field>
          )}
        />
      </FieldGroup>

      {mutation.isError && (
        <p className="text-sm text-destructive" data-testid="package-item-error">
          {(mutation.error as Error).message}
        </p>
      )}

      <div className="flex justify-end">
        <Button type="submit" disabled={mutation.isPending}>
          {mutation.isPending ? "Saving..." : item ? "Save layer" : "Add layer"}
        </Button>
      </div>
    </form>
  );
}
