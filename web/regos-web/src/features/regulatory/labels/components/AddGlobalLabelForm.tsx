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

import { useCreateGlobalLabel } from "../hooks/useCreateGlobalLabel";
import { useLabelVocabulary } from "../hooks/useLabelVocabulary";
import {
  createGlobalLabelSchema,
  type CreateGlobalLabelFormValues,
} from "../validation/createGlobalLabelSchema";

interface AddGlobalLabelFormProps {
  globalProductId: string;
  onSuccess(): void;
}

/**
 * Starts holding a label for a product.
 *
 * **There is no version field.** The first draft is opened with the label, and
 * numbering belongs to the aggregate — a form that let someone type "version 4"
 * would be offering to renumber an issue somebody has already cited.
 */
export function AddGlobalLabelForm({
  globalProductId,
  onSuccess,
}: AddGlobalLabelFormProps) {
  const mutation = useCreateGlobalLabel(globalProductId);
  const { data: vocabulary, isLoading: loadingVocabulary } =
    useLabelVocabulary();

  const {
    control,
    handleSubmit,
    reset,
    formState: { errors },
  } = useForm<CreateGlobalLabelFormValues>({
    resolver: zodResolver(createGlobalLabelSchema),
    defaultValues: {
      name: "",
      labelTypeCode: "CCDS",
    },
  });

  async function onSubmit(values: CreateGlobalLabelFormValues) {
    try {
      await mutation.mutateAsync({
        name: values.name,
        labelTypeCode: values.labelTypeCode,
      });
    } catch {
      // A refusal is an outcome, not a crash. The server's reason renders below
      // and the form keeps what was typed.
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

              <Input
                id="name"
                placeholder="e.g. Company Core Data Sheet"
                {...field}
              />

              <p className="text-xs text-muted-foreground">
                What this document is called internally.
              </p>

              <FieldError errors={[errors.name]} />
            </Field>
          )}
        />

        <Controller
          control={control}
          name="labelTypeCode"
          render={({ field }) => (
            <Field data-invalid={!!errors.labelTypeCode}>
              <FieldLabel htmlFor="labelTypeCode">Label type</FieldLabel>

              <Select
                onValueChange={field.onChange}
                value={field.value}
                disabled={loadingVocabulary}
              >
                <SelectTrigger id="labelTypeCode">
                  <SelectValue placeholder="Select a label type" />
                </SelectTrigger>

                <SelectContent>
                  {(vocabulary?.globalLabelTypes ?? []).map((concept) => (
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
      </FieldGroup>

      {mutation.isError && (
        <p
          className="text-sm text-destructive"
          data-testid="add-global-label-error"
        >
          {(mutation.error as Error).message}
        </p>
      )}

      <div className="flex justify-end gap-2">
        <Button type="submit" disabled={mutation.isPending}>
          {mutation.isPending ? "Adding..." : "Add label"}
        </Button>
      </div>
    </form>
  );
}
