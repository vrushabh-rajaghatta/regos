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

import { LANGUAGES } from "../constants/languages";
import { useAddTradeName } from "../hooks/useAddTradeName";
import {
  addTradeNameSchema,
  type AddTradeNameFormValues,
} from "../validation/addTradeNameSchema";

interface AddTradeNameFormProps {
  medicinalProductId: string;
  onSuccess(): void;
}

/**
 * One name per language, and the server enforces it. The form does not filter
 * out languages already used: the refusal carries the reason in the domain's
 * own words, and hiding the option would leave a user wondering where their
 * language went.
 */
export function AddTradeNameForm({
  medicinalProductId,
  onSuccess,
}: AddTradeNameFormProps) {
  const mutation = useAddTradeName(medicinalProductId);

  const {
    control,
    handleSubmit,
    reset,
    formState: { errors },
  } = useForm<AddTradeNameFormValues>({
    resolver: zodResolver(addTradeNameSchema),
    defaultValues: { language: "", name: "" },
  });

  async function onSubmit(values: AddTradeNameFormValues) {
    try {
      await mutation.mutateAsync(values);
    } catch {
      // A refusal is an outcome, not a crash — the server's reason is rendered
      // from mutation.error below and the form keeps what was typed.
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
          name="language"
          render={({ field }) => (
            <Field data-invalid={!!errors.language}>
              <FieldLabel htmlFor="tradeNameLanguage">Language</FieldLabel>

              <select
                id="tradeNameLanguage"
                className="h-9 w-full rounded-md border bg-transparent px-3 text-sm"
                {...field}
              >
                <option value="">Select a language</option>
                {LANGUAGES.map((language) => (
                  <option key={language.code} value={language.code}>
                    {language.name}
                  </option>
                ))}
              </select>

              {errors.language && (
                <FieldError>{errors.language.message}</FieldError>
              )}
            </Field>
          )}
        />

        <Controller
          control={control}
          name="name"
          render={({ field }) => (
            <Field data-invalid={!!errors.name}>
              <FieldLabel htmlFor="tradeNameName">Trade name</FieldLabel>

              <Input
                id="tradeNameName"
                placeholder="e.g. Cardiolex"
                {...field}
              />

              {errors.name && <FieldError>{errors.name.message}</FieldError>}
            </Field>
          )}
        />
      </FieldGroup>

      {mutation.isError && (
        <p
          className="text-sm text-destructive"
          data-testid="add-trade-name-error"
        >
          {(mutation.error as Error).message}
        </p>
      )}

      <div className="flex justify-end">
        <Button type="submit" disabled={mutation.isPending}>
          {mutation.isPending ? "Saving..." : "Save"}
        </Button>
      </div>
    </form>
  );
}
