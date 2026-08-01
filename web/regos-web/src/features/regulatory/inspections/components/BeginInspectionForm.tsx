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

import { useAuthorities } from "../../masterData/hooks/useAuthorities";
import { useBeginInspection } from "../hooks/useBeginInspection";
import {
  beginInspectionSchema,
  type BeginInspectionFormValues,
} from "../validation/beginInspectionSchema";

interface BeginInspectionFormProps {
  onSuccess(): void;
}

export function BeginInspectionForm({ onSuccess }: BeginInspectionFormProps) {
  const authorities = useAuthorities();
  const mutation = useBeginInspection();

  const {
    control,
    handleSubmit,
    reset,
    formState: { errors },
  } = useForm<BeginInspectionFormValues>({
    resolver: zodResolver(beginInspectionSchema),
    defaultValues: {
      authorityId: "",
      title: "",
      initialStatus: "Announced",
      occurredOn: "",
      scheduledFor: "",
    },
  });

  async function onSubmit(values: BeginInspectionFormValues) {
    try {
      await mutation.mutateAsync({
        authorityId: values.authorityId,
        title: values.title,
        initialStatus: values.initialStatus,
        occurredOn: values.occurredOn,
        scheduledFor: values.scheduledFor || null,
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
          name="authorityId"
          render={({ field }) => (
            <Field data-invalid={!!errors.authorityId}>
              <FieldLabel htmlFor="inspectionAuthorityId">
                Health authority
              </FieldLabel>

              <select
                id="inspectionAuthorityId"
                className="h-9 w-full rounded-md border bg-transparent px-3 text-sm"
                {...field}
              >
                <option value="">Select an authority</option>
                {(authorities.data ?? []).map((authority) => (
                  <option key={authority.id} value={authority.id}>
                    {authority.name}
                  </option>
                ))}
              </select>

              {errors.authorityId && (
                <FieldError>{errors.authorityId.message}</FieldError>
              )}
            </Field>
          )}
        />

        <Controller
          control={control}
          name="initialStatus"
          render={({ field }) => (
            <Field data-invalid={!!errors.initialStatus}>
              <FieldLabel htmlFor="inspectionInitialStatus">
                How we learned of it
              </FieldLabel>

              <select
                id="inspectionInitialStatus"
                className="h-9 w-full rounded-md border bg-transparent px-3 text-sm"
                {...field}
              >
                <option value="Announced">They announced it</option>
                <option value="InProgress">They arrived unannounced</option>
              </select>

              {errors.initialStatus && (
                <FieldError>{errors.initialStatus.message}</FieldError>
              )}
            </Field>
          )}
        />

        <Controller
          control={control}
          name="title"
          render={({ field }) => (
            <Field data-invalid={!!errors.title}>
              <FieldLabel htmlFor="inspectionTitle">What it is</FieldLabel>

              <Input id="inspectionTitle" {...field} />

              {errors.title && <FieldError>{errors.title.message}</FieldError>}
            </Field>
          )}
        />

        <Controller
          control={control}
          name="occurredOn"
          render={({ field }) => (
            <Field data-invalid={!!errors.occurredOn}>
              <FieldLabel htmlFor="inspectionOccurredOn">Learned on</FieldLabel>

              <Input id="inspectionOccurredOn" type="date" {...field} />

              {errors.occurredOn && (
                <FieldError>{errors.occurredOn.message}</FieldError>
              )}
            </Field>
          )}
        />

        <Controller
          control={control}
          name="scheduledFor"
          render={({ field }) => (
            <Field data-invalid={!!errors.scheduledFor}>
              <FieldLabel htmlFor="inspectionScheduledFor">
                Scheduled for (optional)
              </FieldLabel>

              <Input id="inspectionScheduledFor" type="date" {...field} />

              {errors.scheduledFor && (
                <FieldError>{errors.scheduledFor.message}</FieldError>
              )}
            </Field>
          )}
        />
      </FieldGroup>

      {mutation.isError && (
        <p className="text-sm text-destructive" data-testid="begin-inspection-error">
          {(mutation.error as Error).message}
        </p>
      )}

      <div className="flex justify-end">
        <Button type="submit" disabled={mutation.isPending}>
          {mutation.isPending ? "Recording..." : "Record"}
        </Button>
      </div>
    </form>
  );
}
