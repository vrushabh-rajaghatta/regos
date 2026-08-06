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
import { useCountries } from "@/features/regulatory/masterData/hooks/useCountries";
import { useProducts } from "@/features/regulatory/products/hooks/useProducts";

import { useCreateObjective } from "../hooks/useCreateObjective";
import {
  stateObjectiveSchema,
  type StateObjectiveValues,
} from "../validation/stateObjectiveSchema";

interface StateObjectiveFormProps {
  onSuccess(): void;
}

/**
 * A product, a market, and what we are trying to achieve there.
 *
 * **The market is a country, not a market record.** An objective routinely
 * exists before the market-local regulatory product does — *"file in Japan in
 * FY2028"* is a real objective on a market RegOS holds no record for — so this
 * form deliberately offers every country rather than the markets already set up.
 *
 * **And there is no status field.** A new objective is always Proposed;
 * deciding to pursue it is a second, dated event on the detail page.
 */
export function StateObjectiveForm({ onSuccess }: StateObjectiveFormProps) {
  const mutation = useCreateObjective();

  // A picker, not a browser: one page large enough to hold a tenant's
  // portfolio. Paging a select would hide products behind a control the
  // user cannot see.
  const { data: products } = useProducts({ pageSize: 200 });
  const { data: countries } = useCountries();

  const {
    control,
    handleSubmit,
    reset,
    formState: { errors },
  } = useForm<StateObjectiveValues>({
    resolver: zodResolver(stateObjectiveSchema),
    defaultValues: {
      globalProductId: "",
      countryId: "",
      name: "",
      rationale: "",
      targetCompletionOn: "",
    },
  });

  async function onSubmit(values: StateObjectiveValues) {
    try {
      await mutation.mutateAsync({
        globalProductId: values.globalProductId,
        countryId: values.countryId,
        name: values.name,
        statedOn: new Date().toISOString().slice(0, 10),
        rationale: values.rationale?.trim() ? values.rationale : null,
        ownerUserId: null,
        targetCompletionOn: values.targetCompletionOn || null,
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
          name="globalProductId"
          render={({ field }) => (
            <Field data-invalid={!!errors.globalProductId}>
              <FieldLabel htmlFor="globalProductId">Product</FieldLabel>

              <Select onValueChange={field.onChange} value={field.value}>
                <SelectTrigger id="globalProductId">
                  <SelectValue placeholder="Select a product" />
                </SelectTrigger>

                <SelectContent>
                  {products?.items.map((product) => (
                    <SelectItem key={product.id} value={product.id}>
                      {product.name}
                    </SelectItem>
                  ))}
                </SelectContent>
              </Select>

              <FieldError errors={[errors.globalProductId]} />
            </Field>
          )}
        />

        <Controller
          control={control}
          name="countryId"
          render={({ field }) => (
            <Field data-invalid={!!errors.countryId}>
              <FieldLabel htmlFor="countryId">Market</FieldLabel>

              <Select onValueChange={field.onChange} value={field.value}>
                <SelectTrigger id="countryId">
                  <SelectValue placeholder="Select a market" />
                </SelectTrigger>

                <SelectContent>
                  {countries?.map((country) => (
                    <SelectItem key={country.id} value={country.id}>
                      {country.name}
                    </SelectItem>
                  ))}
                </SelectContent>
              </Select>

              <p className="text-xs text-muted-foreground">
                Every country, not only the markets already set up — an objective
                usually comes first.
              </p>

              <FieldError errors={[errors.countryId]} />
            </Field>
          )}
        />

        <Controller
          control={control}
          name="name"
          render={({ field }) => (
            <Field data-invalid={!!errors.name}>
              <FieldLabel htmlFor="name">Objective</FieldLabel>

              <Input
                id="name"
                placeholder="e.g. Obtain approval in Japan"
                {...field}
              />

              <FieldError errors={[errors.name]} />
            </Field>
          )}
        />

        <Controller
          control={control}
          name="rationale"
          render={({ field }) => (
            <Field data-invalid={!!errors.rationale}>
              <FieldLabel htmlFor="rationale">Rationale</FieldLabel>

              <textarea
                id="rationale"
                rows={3}
                placeholder="Why this, and why this route"
                className="w-full rounded-md border bg-transparent p-3 text-sm"
                {...field}
              />

              <FieldError errors={[errors.rationale]} />
            </Field>
          )}
        />

        <Controller
          control={control}
          name="targetCompletionOn"
          render={({ field }) => (
            <Field data-invalid={!!errors.targetCompletionOn}>
              <FieldLabel htmlFor="targetCompletionOn">
                Target completion
              </FieldLabel>

              <Input id="targetCompletionOn" type="date" {...field} />

              <p className="text-xs text-muted-foreground">
                An intention, not a schedule. A plan holds the schedule.
              </p>

              <FieldError errors={[errors.targetCompletionOn]} />
            </Field>
          )}
        />
      </FieldGroup>

      {mutation.isError && (
        <p className="text-sm text-destructive" role="alert">
          {mutation.error.message}
        </p>
      )}

      <Button type="submit" disabled={mutation.isPending}>
        {mutation.isPending ? "Stating..." : "State objective"}
      </Button>
    </form>
  );
}
