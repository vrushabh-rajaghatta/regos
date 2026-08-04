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

import { useCreateLocalLabel } from "../hooks/useCreateLocalLabel";
import { useLabelVocabulary } from "../hooks/useLabelVocabulary";
import {
  createLocalLabelSchema,
  type CreateLocalLabelFormValues,
} from "../validation/createLocalLabelSchema";

interface AddLocalLabelFormProps {
  medicinalProductId: string;
  onSuccess(): void;
}

/**
 * Starts holding a controlled labelling document for this market.
 *
 * **Carton artwork is in the same picker as the leaflet** — it is approved,
 * revised and derived exactly as the others are (EPIC-018 D2).
 */
export function AddLocalLabelForm({
  medicinalProductId,
  onSuccess,
}: AddLocalLabelFormProps) {
  const mutation = useCreateLocalLabel(medicinalProductId);
  const { data: vocabulary, isLoading } = useLabelVocabulary();

  const {
    control,
    handleSubmit,
    reset,
    formState: { errors },
  } = useForm<CreateLocalLabelFormValues>({
    resolver: zodResolver(createLocalLabelSchema),
    defaultValues: { labelTypeCode: "SMPC", language: "en" },
  });

  async function onSubmit(values: CreateLocalLabelFormValues) {
    try {
      await mutation.mutateAsync(values);
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
          name="labelTypeCode"
          render={({ field }) => (
            <Field data-invalid={!!errors.labelTypeCode}>
              <FieldLabel htmlFor="labelTypeCode">Document</FieldLabel>

              <Select
                onValueChange={field.onChange}
                value={field.value}
                disabled={isLoading}
              >
                <SelectTrigger id="labelTypeCode">
                  <SelectValue placeholder="Select a document" />
                </SelectTrigger>

                <SelectContent>
                  {(vocabulary?.localLabelTypes ?? []).map((concept) => (
                    <SelectItem key={concept.code} value={concept.code}>
                      {concept.display}
                    </SelectItem>
                  ))}
                </SelectContent>
              </Select>

              <FieldError errors={[errors.labelTypeCode]} />
            </Field>
          )}
        />

        <Controller
          control={control}
          name="language"
          render={({ field }) => (
            <Field data-invalid={!!errors.language}>
              <FieldLabel htmlFor="language">Language</FieldLabel>

              <Input id="language" placeholder="e.g. ja" {...field} />

              <p className="text-xs text-muted-foreground">
                A market with two languages holds two labels — each is approved
                separately.
              </p>

              <FieldError errors={[errors.language]} />
            </Field>
          )}
        />
      </FieldGroup>

      {mutation.isError && (
        <p
          className="text-sm text-destructive"
          data-testid="add-local-label-error"
        >
          {(mutation.error as Error).message}
        </p>
      )}

      <div className="flex justify-end gap-2">
        <Button type="submit" disabled={mutation.isPending}>
          {mutation.isPending ? "Adding..." : "Add local label"}
        </Button>
      </div>
    </form>
  );
}
