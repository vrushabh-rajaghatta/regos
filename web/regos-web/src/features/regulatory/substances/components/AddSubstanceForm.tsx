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

import { useCreateSubstance } from "../hooks/useCreateSubstance";
import { useSubstanceVocabulary } from "../hooks/useSubstanceVocabulary";
import {
  createSubstanceSchema,
  type CreateSubstanceFormValues,
} from "../validation/createSubstanceSchema";

interface AddSubstanceFormProps {
  onSuccess(): void;
}

/**
 * Adds a compound to this organisation's half of the catalogue.
 *
 * **INN is optional and that is the point** — an innovator holds a molecule
 * before anyone assigns it an International Nonproprietary Name, and that
 * absence is exactly the case a proprietary substance exists to record.
 */
export function AddSubstanceForm({ onSuccess }: AddSubstanceFormProps) {
  const mutation = useCreateSubstance();
  const { data: vocabulary, isLoading: loadingVocabulary } =
    useSubstanceVocabulary();

  const {
    control,
    handleSubmit,
    reset,
    formState: { errors },
  } = useForm<CreateSubstanceFormValues>({
    resolver: zodResolver(createSubstanceSchema),
    defaultValues: {
      name: "",
      inn: "",
      substanceClassCode: "CHEMICAL",
      substanceTypeCode: "SYNTHETIC",
      casNumber: "",
      uniiCode: "",
      molecularFormula: "",
      description: "",
    },
  });

  async function onSubmit(values: CreateSubstanceFormValues) {
    try {
      await mutation.mutateAsync({
        name: values.name,
        // Blank means absent, not empty. The server collapses it too; sending
        // null keeps the wire honest about which facts are missing.
        inn: values.inn?.trim() || null,
        substanceClassCode: values.substanceClassCode,
        substanceTypeCode: values.substanceTypeCode,
        casNumber: values.casNumber?.trim() || null,
        uniiCode: values.uniiCode?.trim() || null,
        molecularFormula: values.molecularFormula?.trim() || null,
        description: values.description?.trim() || null,
      });
    } catch {
      // A refusal is an outcome, not a crash. The server's reason renders below
      // and the form keeps what was typed — a name already in the shared
      // catalogue usually means reaching for that row instead.
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
              <FieldLabel htmlFor="name">Name</FieldLabel>

              <Input id="name" placeholder="e.g. RGX-1174" {...field} />

              <p className="text-xs text-muted-foreground">
                The preferred scientific name — the one this substance is
                displayed under.
              </p>

              <FieldError errors={[errors.name]} />
            </Field>
          )}
        />

        <Controller
          control={control}
          name="inn"
          render={({ field }) => (
            <Field data-invalid={!!errors.inn}>
              <FieldLabel htmlFor="inn">INN</FieldLabel>

              <Input
                id="inn"
                placeholder="Leave blank if none has been assigned"
                {...field}
              />

              <p className="text-xs text-muted-foreground">
                The WHO International Nonproprietary Name. A compound that has
                not been assigned one yet simply has no INN.
              </p>

              <FieldError errors={[errors.inn]} />
            </Field>
          )}
        />

        <Controller
          control={control}
          name="substanceClassCode"
          render={({ field }) => (
            <Field data-invalid={!!errors.substanceClassCode}>
              <FieldLabel htmlFor="substanceClassCode">Class</FieldLabel>

              <Select
                onValueChange={field.onChange}
                value={field.value}
                disabled={loadingVocabulary}
              >
                <SelectTrigger id="substanceClassCode">
                  <SelectValue placeholder="Select a class" />
                </SelectTrigger>

                <SelectContent>
                  {(vocabulary?.classes ?? []).map((concept) => (
                    <SelectItem key={concept.code} value={concept.code}>
                      {concept.display}
                    </SelectItem>
                  ))}
                </SelectContent>
              </Select>

              <FieldError errors={[errors.substanceClassCode]} />
            </Field>
          )}
        />

        <Controller
          control={control}
          name="substanceTypeCode"
          render={({ field }) => (
            <Field data-invalid={!!errors.substanceTypeCode}>
              <FieldLabel htmlFor="substanceTypeCode">Type</FieldLabel>

              <Select
                onValueChange={field.onChange}
                value={field.value}
                disabled={loadingVocabulary}
              >
                <SelectTrigger id="substanceTypeCode">
                  <SelectValue placeholder="Select a type" />
                </SelectTrigger>

                <SelectContent>
                  {(vocabulary?.types ?? []).map((concept) => (
                    <SelectItem key={concept.code} value={concept.code}>
                      {concept.display}
                    </SelectItem>
                  ))}
                </SelectContent>
              </Select>

              <FieldError errors={[errors.substanceTypeCode]} />
            </Field>
          )}
        />

        <Controller
          control={control}
          name="casNumber"
          render={({ field }) => (
            <Field data-invalid={!!errors.casNumber}>
              <FieldLabel htmlFor="casNumber">CAS number</FieldLabel>

              <Input id="casNumber" placeholder="Optional" {...field} />

              <FieldError errors={[errors.casNumber]} />
            </Field>
          )}
        />

        <Controller
          control={control}
          name="uniiCode"
          render={({ field }) => (
            <Field data-invalid={!!errors.uniiCode}>
              <FieldLabel htmlFor="uniiCode">UNII</FieldLabel>

              <Input id="uniiCode" placeholder="Optional" {...field} />

              <p className="text-xs text-muted-foreground">
                RegOS does not hold the GSRS registry, so this is recorded as
                given and is not verified.
              </p>

              <FieldError errors={[errors.uniiCode]} />
            </Field>
          )}
        />

        <Controller
          control={control}
          name="molecularFormula"
          render={({ field }) => (
            <Field data-invalid={!!errors.molecularFormula}>
              <FieldLabel htmlFor="molecularFormula">
                Molecular formula
              </FieldLabel>

              <Input
                id="molecularFormula"
                placeholder="e.g. C8H9NO2"
                {...field}
              />

              <FieldError errors={[errors.molecularFormula]} />
            </Field>
          )}
        />

        <Controller
          control={control}
          name="description"
          render={({ field }) => (
            <Field data-invalid={!!errors.description}>
              <FieldLabel htmlFor="description">Description</FieldLabel>

              <Input id="description" placeholder="Optional" {...field} />

              <FieldError errors={[errors.description]} />
            </Field>
          )}
        />
      </FieldGroup>

      {mutation.isError && (
        <p
          className="text-sm text-destructive"
          data-testid="add-substance-error"
        >
          {(mutation.error as Error).message}
        </p>
      )}

      <div className="flex justify-end gap-2">
        <Button type="submit" disabled={mutation.isPending}>
          {mutation.isPending ? "Adding..." : "Add substance"}
        </Button>
      </div>
    </form>
  );
}
