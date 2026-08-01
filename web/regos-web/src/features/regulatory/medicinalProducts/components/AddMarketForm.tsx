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

import { useCountries } from "../../masterData/hooks/useCountries";
import { useCreateMedicinalProduct } from "../hooks/useCreateMedicinalProduct";
import {
  addMarketSchema,
  type AddMarketFormValues,
} from "../validation/addMarketSchema";

interface AddMarketFormProps {
  globalProductId: string;
  onSuccess(): void;
}

/**
 * Adding a market is its own act, not a side effect of recording a licence.
 * A company decides to market somewhere long before an authority agrees, and
 * the same country can legitimately be added twice — different presentations,
 * or the two halves of a partial divestment — so nothing here refuses a
 * duplicate.
 */
export function AddMarketForm({
  globalProductId,
  onSuccess,
}: AddMarketFormProps) {
  const countries = useCountries();
  const mutation = useCreateMedicinalProduct(globalProductId);

  const {
    control,
    handleSubmit,
    reset,
    formState: { errors },
  } = useForm<AddMarketFormValues>({
    resolver: zodResolver(addMarketSchema),
    defaultValues: { countryId: "", statusDate: "" },
  });

  async function onSubmit(values: AddMarketFormValues) {
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
          name="countryId"
          render={({ field }) => (
            <Field data-invalid={!!errors.countryId}>
              <FieldLabel htmlFor="marketCountryId">Country</FieldLabel>

              <select
                id="marketCountryId"
                className="h-9 w-full rounded-md border bg-transparent px-3 text-sm"
                {...field}
              >
                <option value="">Select a country</option>
                {(countries.data ?? []).map((country) => (
                  <option key={country.id} value={country.id}>
                    {country.name}
                  </option>
                ))}
              </select>

              {errors.countryId && (
                <FieldError>{errors.countryId.message}</FieldError>
              )}
            </Field>
          )}
        />

        <Controller
          control={control}
          name="statusDate"
          render={({ field }) => (
            <Field data-invalid={!!errors.statusDate}>
              <FieldLabel htmlFor="marketStatusDate">Present since</FieldLabel>

              <Input id="marketStatusDate" type="date" {...field} />

              {errors.statusDate && (
                <FieldError>{errors.statusDate.message}</FieldError>
              )}
            </Field>
          )}
        />
      </FieldGroup>

      {mutation.isError && (
        <p className="text-sm text-destructive" data-testid="add-market-error">
          {(mutation.error as Error).message}
        </p>
      )}

      <div className="flex justify-end">
        {/* "Add", not "Add market": the page's own action is called "Add
            market", and two buttons with one name is a page that cannot be
            described unambiguously — to a test or to a screen reader. */}
        <Button type="submit" disabled={mutation.isPending}>
          {mutation.isPending ? "Adding..." : "Add"}
        </Button>
      </div>
    </form>
  );
}
