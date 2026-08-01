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

import { MARKET_STATUSES } from "../constants/marketStatuses";
import { useChangeMarketStatus } from "../hooks/useChangeMarketStatus";
import {
  changeMarketStatusSchema,
  type ChangeMarketStatusFormValues,
} from "../validation/changeMarketStatusSchema";

interface ChangeMarketStatusFormProps {
  medicinalProductId: string;
  onSuccess(): void;
}

/**
 * Records what became commercially true, and when.
 *
 * Every option is always offered, because there is no transition table to
 * consult — a market may be launched, lost and launched again. `Planned` is
 * absent from the list rather than disabled: it is the state a market begins
 * in, and one already entered cannot be intended again.
 */
export function ChangeMarketStatusForm({
  medicinalProductId,
  onSuccess,
}: ChangeMarketStatusFormProps) {
  const mutation = useChangeMarketStatus(medicinalProductId);

  const {
    control,
    handleSubmit,
    reset,
    formState: { errors },
  } = useForm<ChangeMarketStatusFormValues>({
    resolver: zodResolver(changeMarketStatusSchema),
    defaultValues: { status: "Launched", occurredOn: "", note: "" },
  });

  async function onSubmit(values: ChangeMarketStatusFormValues) {
    try {
      await mutation.mutateAsync({
        status: values.status,
        occurredOn: values.occurredOn,
        note: values.note === "" ? null : values.note,
      });
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
          name="status"
          render={({ field }) => (
            <Field data-invalid={!!errors.status}>
              <FieldLabel htmlFor="marketStatus">Now</FieldLabel>

              <select
                id="marketStatus"
                className="h-9 w-full rounded-md border bg-transparent px-3 text-sm"
                {...field}
              >
                {MARKET_STATUSES.map((option) => (
                  <option key={option.value} value={option.value}>
                    {option.label}
                  </option>
                ))}
              </select>

              {errors.status && (
                <FieldError>{errors.status.message}</FieldError>
              )}
            </Field>
          )}
        />

        <Controller
          control={control}
          name="occurredOn"
          render={({ field }) => (
            <Field data-invalid={!!errors.occurredOn}>
              <FieldLabel htmlFor="marketStatusOccurredOn">
                Took effect on
              </FieldLabel>

              <Input id="marketStatusOccurredOn" type="date" {...field} />

              {errors.occurredOn && (
                <FieldError>{errors.occurredOn.message}</FieldError>
              )}
            </Field>
          )}
        />

        <Controller
          control={control}
          name="note"
          render={({ field }) => (
            <Field>
              <FieldLabel htmlFor="marketStatusNote">
                Note (optional)
              </FieldLabel>

              <Input
                id="marketStatusNote"
                placeholder="API supply interruption."
                {...field}
              />
            </Field>
          )}
        />
      </FieldGroup>

      {mutation.isError && (
        <p
          className="text-sm text-destructive"
          data-testid="market-status-error"
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
