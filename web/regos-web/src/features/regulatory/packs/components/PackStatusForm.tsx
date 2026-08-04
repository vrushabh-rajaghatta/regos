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

import { PACK_STATUSES } from "../constants/packStatuses";
import { useChangePackMarketingStatus } from "../hooks/useChangePackMarketingStatus";
import {
  packMarketingStatusSchema,
  type PackMarketingStatusFormValues,
} from "../validation/packSchema";

interface PackStatusFormProps {
  medicinalProductId: string;
  packagedProductId: string;
  onSuccess(): void;
}

export function PackStatusForm({
  medicinalProductId,
  packagedProductId,
  onSuccess,
}: PackStatusFormProps) {
  const mutation = useChangePackMarketingStatus(medicinalProductId);

  const {
    control,
    handleSubmit,
    reset,
    formState: { errors },
  } = useForm<PackMarketingStatusFormValues>({
    resolver: zodResolver(packMarketingStatusSchema),
    defaultValues: { status: "Marketed", occurredOn: "", note: "" },
  });

  async function onSubmit(values: PackMarketingStatusFormValues) {
    try {
      await mutation.mutateAsync({
        packagedProductId,
        body: {
          status: values.status,
          occurredOn: values.occurredOn,
          note: values.note === "" ? null : values.note,
        },
      });
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
          name="status"
          render={({ field }) => (
            <Field data-invalid={!!errors.status}>
              <FieldLabel htmlFor="packStatus">Now</FieldLabel>

              <select
                id="packStatus"
                className="h-9 w-full rounded-md border bg-transparent px-3 text-sm"
                {...field}
              >
                {PACK_STATUSES.map((option) => (
                  <option key={option.value} value={option.value}>
                    {option.label}
                  </option>
                ))}
              </select>

              {errors.status && <FieldError>{errors.status.message}</FieldError>}
            </Field>
          )}
        />

        <Controller
          control={control}
          name="occurredOn"
          render={({ field }) => (
            <Field data-invalid={!!errors.occurredOn}>
              <FieldLabel htmlFor="packStatusOccurredOn">
                Took effect on
              </FieldLabel>

              <Input id="packStatusOccurredOn" type="date" {...field} />

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
              <FieldLabel htmlFor="packStatusNote">Note (optional)</FieldLabel>

              <Input
                id="packStatusNote"
                placeholder="Artwork changeover."
                {...field}
              />
            </Field>
          )}
        />
      </FieldGroup>

      {mutation.isError && (
        <p className="text-sm text-destructive" data-testid="pack-status-error">
          {(mutation.error as Error).message}
        </p>
      )}

      <div className="flex justify-end">
        <Button type="submit" disabled={mutation.isPending}>
          {mutation.isPending ? "Saving..." : "Save status"}
        </Button>
      </div>
    </form>
  );
}
