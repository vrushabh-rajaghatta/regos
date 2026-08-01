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

import { useSetMarketActivation } from "../hooks/useSetMarketActivation";
import {
  marketActivationSchema,
  type MarketActivationFormValues,
} from "../validation/marketActivationSchema";

interface MarketActivationFormProps {
  medicinalProductId: string;
  /** True when restoring a retired record, false when retiring an active one. */
  active: boolean;
  registrationCount: number;
  onSuccess(): void;
}

/**
 * Retiring a market record excludes it from normal work. It is not a
 * regulatory or commercial event — the licences beneath it stay valid and the
 * product stays on sale if it was.
 *
 * When registrations exist the form <em>warns</em> and proceeds. That is
 * deliberate: warnings help humans, domain rules preserve truth, and nothing
 * here makes a truth impossible to represent. A rule refusing this would also
 * have to reach into another aggregate's lifecycle to decide which
 * registrations counted, which is the evidence it belongs elsewhere.
 */
export function MarketActivationForm({
  medicinalProductId,
  active,
  registrationCount,
  onSuccess,
}: MarketActivationFormProps) {
  const mutation = useSetMarketActivation(medicinalProductId);

  const {
    control,
    handleSubmit,
    reset,
    formState: { errors },
  } = useForm<MarketActivationFormValues>({
    resolver: zodResolver(marketActivationSchema),
    defaultValues: { on: "" },
  });

  async function onSubmit(values: MarketActivationFormValues) {
    try {
      await mutation.mutateAsync({ active, on: values.on });
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
      <p className="text-sm text-muted-foreground">
        {active
          ? "This market record returns to normal work. Nothing else changes."
          : "This market record is kept, but excluded from normal work. Its "
            + "authorisations stay valid and its sale status is unchanged."}
      </p>

      {!active && registrationCount > 0 && (
        <p className="text-sm text-destructive" data-testid="retire-warning">
          This market holds {registrationCount}{" "}
          {registrationCount === 1 ? "authorisation" : "authorisations"}. They
          remain valid — but they will no longer appear in day-to-day work.
        </p>
      )}

      <FieldGroup>
        <Controller
          control={control}
          name="on"
          render={({ field }) => (
            <Field data-invalid={!!errors.on}>
              <FieldLabel htmlFor="marketActivationOn">
                {active ? "Restored on" : "Retired on"}
              </FieldLabel>

              <Input id="marketActivationOn" type="date" {...field} />

              {errors.on && <FieldError>{errors.on.message}</FieldError>}
            </Field>
          )}
        />
      </FieldGroup>

      {mutation.isError && (
        <p
          className="text-sm text-destructive"
          data-testid="market-activation-error"
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
