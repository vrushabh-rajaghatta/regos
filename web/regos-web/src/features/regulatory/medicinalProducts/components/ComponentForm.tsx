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

import { useAddComponent } from "../hooks/useAddComponent";
import { usePharmaceuticalVocabulary } from "../hooks/usePharmaceuticalVocabulary";
import { useRestateComponent } from "../hooks/useRestateComponent";
import type { Component } from "../types/Component";
import {
  componentSchema,
  type ComponentFormValues,
} from "../validation/componentSchema";
import { NO_UNIT } from "../validation/ingredientSchema";

interface ComponentFormProps {
  medicinalProductId: string;
  /** Present when correcting one, absent when adding. */
  component?: Component;
  /** Where a new one goes. Null puts it at the top level. */
  parentComponentId?: string | null;
  parentName?: string;
  onSuccess(): void;
}

/**
 * An article, what kind it is, and how many of it.
 *
 * **Where it sits is not here.** Position is chosen by which "Add inside"
 * button was pressed, and changed by the move control on the row — because
 * moving a component is the operation the cycle and depth rules are attached
 * to, and folding it into a general form would hide that.
 */
export function ComponentForm({
  medicinalProductId,
  component,
  parentComponentId = null,
  parentName,
  onSuccess,
}: ComponentFormProps) {
  const add = useAddComponent(medicinalProductId);
  const restate = useRestateComponent(medicinalProductId);

  const mutation = component ? restate : add;

  const { data: vocabulary, isLoading } = usePharmaceuticalVocabulary();

  const {
    control,
    handleSubmit,
    reset,
    formState: { errors },
  } = useForm<ComponentFormValues>({
    resolver: zodResolver(componentSchema),
    defaultValues: {
      componentTypeCode: component?.componentType.code ?? "",
      name: component?.name ?? "",
      description: component?.description ?? "",
      quantity: component ? String(component.quantity) : "1",
      unitOfPresentationCode: component?.unitOfPresentation?.code ?? NO_UNIT,
      doseFormCode: component?.doseForm?.code ?? NO_UNIT,
    },
  });

  async function onSubmit(values: ComponentFormValues) {
    const code = (value?: string) =>
      value && value !== NO_UNIT ? value : null;

    const body = {
      componentTypeCode: values.componentTypeCode,
      name: values.name,
      description: values.description?.trim() || null,
      quantity: Number(values.quantity),
      unitOfPresentationCode: code(values.unitOfPresentationCode),
      doseFormCode: code(values.doseFormCode),
    };

    try {
      if (component) {
        await restate.mutateAsync({ ...body, componentId: component.componentId });
      } else {
        await add.mutateAsync({ ...body, parentComponentId });
      }
    } catch {
      // A refusal is an outcome, not a crash — the server's reason renders
      // below, and for a placement it names the depth limit.
      return;
    }

    reset();

    onSuccess();
  }

  return (
    <form onSubmit={handleSubmit(onSubmit)} className="space-y-6">
      {!component && parentName && (
        <p className="text-sm text-muted-foreground">
          Going inside <strong>{parentName}</strong>.
        </p>
      )}

      <FieldGroup>
        <Controller
          control={control}
          name="componentTypeCode"
          render={({ field }) => (
            <Field data-invalid={!!errors.componentTypeCode}>
              <FieldLabel htmlFor="component-type">Type</FieldLabel>

              <Select
                onValueChange={field.onChange}
                value={field.value}
                disabled={isLoading}
              >
                <SelectTrigger id="component-type">
                  <SelectValue placeholder="Select a type" />
                </SelectTrigger>

                <SelectContent>
                  {(vocabulary?.componentTypes ?? []).map((concept) => (
                    <SelectItem key={concept.code} value={concept.code}>
                      {concept.display}
                    </SelectItem>
                  ))}
                </SelectContent>
              </Select>

              <FieldError errors={[errors.componentTypeCode]} />
            </Field>
          )}
        />

        <Controller
          control={control}
          name="name"
          render={({ field }) => (
            <Field data-invalid={!!errors.name}>
              <FieldLabel htmlFor="component-name">Name</FieldLabel>

              <Input
                id="component-name"
                placeholder="e.g. Vial of powder"
                {...field}
              />

              <FieldError errors={[errors.name]} />
            </Field>
          )}
        />

        <Controller
          control={control}
          name="quantity"
          render={({ field }) => (
            <Field data-invalid={!!errors.quantity}>
              <FieldLabel htmlFor="component-quantity">Quantity</FieldLabel>

              <Input
                id="component-quantity"
                inputMode="decimal"
                placeholder="1"
                {...field}
              />

              <p className="text-xs text-muted-foreground">
                How many of this article the pack holds.
              </p>

              <FieldError errors={[errors.quantity]} />
            </Field>
          )}
        />

        <Controller
          control={control}
          name="doseFormCode"
          render={({ field }) => (
            <Field data-invalid={!!errors.doseFormCode}>
              <FieldLabel htmlFor="component-dose-form">
                Contents form
              </FieldLabel>

              <Select
                onValueChange={field.onChange}
                value={field.value}
                disabled={isLoading}
              >
                <SelectTrigger id="component-dose-form">
                  <SelectValue placeholder="None" />
                </SelectTrigger>

                <SelectContent>
                  <SelectItem value={NO_UNIT}>None</SelectItem>

                  {(vocabulary?.doseForms ?? []).map((concept) => (
                    <SelectItem key={concept.code} value={concept.code}>
                      {concept.display}
                    </SelectItem>
                  ))}
                </SelectContent>
              </Select>

              <p className="text-xs text-muted-foreground">
                What is inside this article — a vial of powder, an ampoule of
                solution. A kit's halves each have their own.
              </p>

              <FieldError errors={[errors.doseFormCode]} />
            </Field>
          )}
        />

        <Controller
          control={control}
          name="description"
          render={({ field }) => (
            <Field data-invalid={!!errors.description}>
              <FieldLabel htmlFor="component-description">
                Description
              </FieldLabel>

              <Input
                id="component-description"
                placeholder="Optional"
                {...field}
              />

              <FieldError errors={[errors.description]} />
            </Field>
          )}
        />
      </FieldGroup>

      {mutation.isError && (
        <p className="text-sm text-destructive" data-testid="component-error">
          {(mutation.error as Error).message}
        </p>
      )}

      <div className="flex justify-end gap-2">
        <Button type="submit" disabled={mutation.isPending}>
          {mutation.isPending
            ? "Saving..."
            : component
              ? "Save component"
              : "Add component"}
        </Button>
      </div>
    </form>
  );
}
