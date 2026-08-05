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

import { useDescribeAppearance } from "../hooks/useDescribeAppearance";
import { usePharmaceuticalVocabulary } from "../hooks/usePharmaceuticalVocabulary";
import type { Presentation } from "../types/Presentation";
import {
  appearanceSchema,
  type AppearanceFormValues,
} from "../validation/appearanceSchema";

interface AppearanceFormProps {
  medicinalProductId: string;
  presentation: Presentation;
  onSuccess(): void;
}

/**
 * What the medicine looks like.
 *
 * **Colour is a set, shape is a choice.** A capsule with a white body and a
 * blue cap is two colours; a tablet is round or it is oval and nothing is both.
 * The form offers each accordingly rather than making one field pretend to be
 * the other.
 */
export function AppearanceForm({
  medicinalProductId,
  presentation,
  onSuccess,
}: AppearanceFormProps) {
  const { data: vocabulary } = usePharmaceuticalVocabulary();

  const mutation = useDescribeAppearance(medicinalProductId);

  const {
    control,
    handleSubmit,
    formState: { errors },
  } = useForm<AppearanceFormValues>({
    resolver: zodResolver(appearanceSchema),
    defaultValues: {
      colourCodes: presentation.appearance.colours.map((x) => x.code),
      shapeCode: presentation.appearance.shape?.code ?? "",
      imprint: presentation.appearance.imprint ?? "",
      description: presentation.appearance.description ?? "",
    },
  });

  async function onSubmit(values: AppearanceFormValues) {
    try {
      await mutation.mutateAsync({
        presentationId: presentation.presentationId,
        colourCodes: values.colourCodes,
        shapeCode: values.shapeCode === "" ? null : values.shapeCode,
        imprint: values.imprint === "" ? null : (values.imprint ?? null),
        description:
          values.description === "" ? null : (values.description ?? null),
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
          name="colourCodes"
          render={({ field }) => (
            <Field>
              {/* A title, not a label: each checkbox carries its own, and a
                  second <label> pointing at nothing would name the group
                  after a control that does not exist. */}
              <FieldTitle>Colours</FieldTitle>

              <div
                className="grid grid-cols-2 gap-x-4 gap-y-1"
                data-testid="appearance-colours"
              >
                {(vocabulary?.colours ?? []).map((concept) => (
                  <div key={concept.code} className="flex items-center gap-2">
                    <input
                      id={`colour-${concept.code}`}
                      type="checkbox"
                      className="size-4"
                      checked={field.value.includes(concept.code)}
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
                      htmlFor={`colour-${concept.code}`}
                      className="text-sm"
                    >
                      {concept.display}
                    </label>
                  </div>
                ))}
              </div>

              <p className="text-xs text-muted-foreground">
                More than one is ordinary — a capsule with a white body and a
                blue cap is two colours.
              </p>
            </Field>
          )}
        />

        <Controller
          control={control}
          name="shapeCode"
          render={({ field }) => (
            <Field>
              <FieldLabel htmlFor="appearanceShape">Shape</FieldLabel>

              <select
                id="appearanceShape"
                className="h-9 w-full rounded-md border bg-transparent px-3 text-sm"
                {...field}
              >
                <option value="">Not stated</option>

                {(vocabulary?.shapes ?? []).map((concept) => (
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
          name="imprint"
          render={({ field }) => (
            <Field data-invalid={!!errors.imprint}>
              <FieldLabel htmlFor="appearanceImprint">Marking</FieldLabel>

              <Input id="appearanceImprint" placeholder="AZ 10" {...field} />

              {errors.imprint && (
                <FieldError>{errors.imprint.message}</FieldError>
              )}

              {/* Why it is not just part of the sentence below. */}
              <p className="text-xs text-muted-foreground">
                What is stamped on it. The one thing somebody holding a loose
                tablet can look it up by.
              </p>
            </Field>
          )}
        />

        <Controller
          control={control}
          name="description"
          render={({ field }) => (
            <Field>
              <FieldLabel htmlFor="appearanceDescription">
                Wording on the label (optional)
              </FieldLabel>

              <Input
                id="appearanceDescription"
                placeholder="White to off-white, round, biconvex film-coated tablet."
                {...field}
              />
            </Field>
          )}
        />
      </FieldGroup>

      {mutation.isError && (
        <p className="text-sm text-destructive" data-testid="appearance-error">
          {(mutation.error as Error).message}
        </p>
      )}

      <div className="flex justify-end">
        <Button type="submit" disabled={mutation.isPending}>
          {mutation.isPending ? "Saving..." : "Save appearance"}
        </Button>
      </div>
    </form>
  );
}
