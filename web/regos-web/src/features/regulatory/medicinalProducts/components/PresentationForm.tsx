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

import { useAddPresentation } from "../hooks/useAddPresentation";
import { usePharmaceuticalVocabulary } from "../hooks/usePharmaceuticalVocabulary";
import { useRestatePresentation } from "../hooks/useRestatePresentation";
import type { Presentation } from "../types/Presentation";
import {
  presentationSchema,
  type PresentationFormValues,
} from "../validation/presentationSchema";

interface PresentationFormProps {
  medicinalProductId: string;
  /** Present when correcting one, absent when adding. */
  presentation?: Presentation;
  onSuccess(): void;
}

const NO_UNIT = "__none__";

/**
 * One form for both adding and correcting.
 *
 * The server takes the same five facts either way, and a presentation that
 * could be restated into a state it could not be created in would be a gap
 * rather than a feature. **Restate replaces the whole statement**, which is why
 * the form opens pre-filled with everything the presentation currently says —
 * submitting a half-filled correction would erase the other half.
 */
export function PresentationForm({
  medicinalProductId,
  presentation,
  onSuccess,
}: PresentationFormProps) {
  const add = useAddPresentation(medicinalProductId);
  const restate = useRestatePresentation(medicinalProductId);

  const mutation = presentation ? restate : add;

  const { data: vocabulary, isLoading: loadingVocabulary } =
    usePharmaceuticalVocabulary();

  const {
    control,
    handleSubmit,
    reset,
    watch,
    setValue,
    formState: { errors },
  } = useForm<PresentationFormValues>({
    resolver: zodResolver(presentationSchema),
    defaultValues: {
      name: presentation?.name ?? "",
      description: presentation?.description ?? "",
      doseFormCode: presentation?.doseForm.code ?? "",
      unitOfPresentationCode: presentation?.unitOfPresentation?.code ?? NO_UNIT,
      routeCodes:
        presentation?.routesOfAdministration.map((route) => route.code) ?? [],
    },
  });

  const selectedRoutes = watch("routeCodes");

  function toggleRoute(code: string) {
    setValue(
      "routeCodes",
      selectedRoutes.includes(code)
        ? selectedRoutes.filter((existing) => existing !== code)
        : [...selectedRoutes, code],
    );
  }

  async function onSubmit(values: PresentationFormValues) {
    const body = {
      name: values.name,
      description: values.description?.trim() || null,
      doseFormCode: values.doseFormCode,
      unitOfPresentationCode:
        values.unitOfPresentationCode === NO_UNIT
          ? null
          : values.unitOfPresentationCode || null,
      routeCodes: values.routeCodes,
    };

    try {
      if (presentation) {
        await restate.mutateAsync({
          ...body,
          presentationId: presentation.presentationId,
        });
      } else {
        await add.mutateAsync(body);
      }
    } catch {
      // A refusal is an outcome, not a crash. The server's reason renders below
      // — it names the words it would have accepted — and the form keeps what
      // was typed.
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
          name="name"
          render={({ field }) => (
            <Field data-invalid={!!errors.name}>
              <FieldLabel htmlFor="presentation-name">Name</FieldLabel>

              <Input
                id="presentation-name"
                placeholder="e.g. Film-coated tablet, 10 mg"
                {...field}
              />

              <p className="text-xs text-muted-foreground">
                How this presentation is told apart from the others in this
                market.
              </p>

              <FieldError errors={[errors.name]} />
            </Field>
          )}
        />

        <Controller
          control={control}
          name="doseFormCode"
          render={({ field }) => (
            <Field data-invalid={!!errors.doseFormCode}>
              <FieldLabel htmlFor="dose-form">Dose form</FieldLabel>

              <Select
                onValueChange={field.onChange}
                value={field.value}
                disabled={loadingVocabulary}
              >
                <SelectTrigger id="dose-form">
                  <SelectValue placeholder="Select a dose form" />
                </SelectTrigger>

                <SelectContent>
                  {(vocabulary?.doseForms ?? []).map((concept) => (
                    <SelectItem key={concept.code} value={concept.code}>
                      {concept.display}
                    </SelectItem>
                  ))}
                </SelectContent>
              </Select>

              <FieldError errors={[errors.doseFormCode]} />
            </Field>
          )}
        />

        <Controller
          control={control}
          name="unitOfPresentationCode"
          render={({ field }) => (
            <Field data-invalid={!!errors.unitOfPresentationCode}>
              <FieldLabel htmlFor="unit-of-presentation">
                Unit of presentation
              </FieldLabel>

              <Select
                onValueChange={field.onChange}
                value={field.value}
                disabled={loadingVocabulary}
              >
                <SelectTrigger id="unit-of-presentation">
                  <SelectValue placeholder="None" />
                </SelectTrigger>

                <SelectContent>
                  <SelectItem value={NO_UNIT}>None</SelectItem>

                  {(vocabulary?.unitsOfPresentation ?? []).map((concept) => (
                    <SelectItem key={concept.code} value={concept.code}>
                      {concept.display}
                    </SelectItem>
                  ))}
                </SelectContent>
              </Select>

              <p className="text-xs text-muted-foreground">
                The article a patient is given. Leave as None when there is
                nothing to count — an oral solution measured in mL.
              </p>

              <FieldError errors={[errors.unitOfPresentationCode]} />
            </Field>
          )}
        />

        <Field>
          <FieldLabel>Routes of administration</FieldLabel>

          {/* Toggles, not a multi-select: several routes is the ordinary case
              — a solution for injection is routinely intravenous and
              intramuscular — and a list you have to hold a modifier key to add
              to hides that. */}
          <div className="flex flex-wrap gap-2" data-testid="route-toggles">
            {(vocabulary?.routesOfAdministration ?? []).map((concept) => (
              <Button
                key={concept.code}
                type="button"
                size="sm"
                variant={
                  selectedRoutes.includes(concept.code) ? "default" : "outline"
                }
                onClick={() => toggleRoute(concept.code)}
                data-testid={`route-${concept.code}`}
              >
                {concept.display}
              </Button>
            ))}
          </div>

          <p className="text-xs text-muted-foreground">
            Several is ordinary. None is fine too, if the route is not settled
            yet.
          </p>
        </Field>

        <Controller
          control={control}
          name="description"
          render={({ field }) => (
            <Field data-invalid={!!errors.description}>
              <FieldLabel htmlFor="presentation-description">
                Description
              </FieldLabel>

              <Input
                id="presentation-description"
                placeholder="Optional"
                {...field}
              />

              <FieldError errors={[errors.description]} />
            </Field>
          )}
        />
      </FieldGroup>

      {mutation.isError && (
        <p
          className="text-sm text-destructive"
          data-testid="presentation-error"
        >
          {(mutation.error as Error).message}
        </p>
      )}

      <div className="flex justify-end gap-2">
        <Button type="submit" disabled={mutation.isPending}>
          {mutation.isPending
            ? "Saving..."
            : presentation
              ? "Save presentation"
              : "Add presentation"}
        </Button>
      </div>
    </form>
  );
}
