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
import { useBeginMeeting } from "../hooks/useBeginMeeting";
import {
  beginMeetingSchema,
  type BeginMeetingFormValues,
} from "../validation/beginMeetingSchema";

interface BeginMeetingFormProps {
  onSuccess(): void;
}

/**
 * A meeting begins one of two ways, and the form asks which.
 *
 * We request some meetings; an authority calls others. Recording the second
 * kind as "requested, then granted" would put a request in the history that
 * never happened.
 */
export function BeginMeetingForm({ onSuccess }: BeginMeetingFormProps) {
  const authorities = useAuthorities();
  const mutation = useBeginMeeting();

  const {
    control,
    handleSubmit,
    reset,
    formState: { errors },
  } = useForm<BeginMeetingFormValues>({
    resolver: zodResolver(beginMeetingSchema),
    defaultValues: {
      authorityId: "",
      subject: "",
      initialStatus: "Requested",
      occurredOn: "",
      scheduledFor: "",
    },
  });

  async function onSubmit(values: BeginMeetingFormValues) {
    try {
      await mutation.mutateAsync({
        authorityId: values.authorityId,
        subject: values.subject,
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
              <FieldLabel htmlFor="meetingAuthorityId">Health authority</FieldLabel>

              <select
                id="meetingAuthorityId"
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
              <FieldLabel htmlFor="meetingInitialStatus">
                Who asked for it
              </FieldLabel>

              <select
                id="meetingInitialStatus"
                className="h-9 w-full rounded-md border bg-transparent px-3 text-sm"
                {...field}
              >
                <option value="Requested">We requested it</option>
                <option value="Granted">The authority called it</option>
              </select>

              {errors.initialStatus && (
                <FieldError>{errors.initialStatus.message}</FieldError>
              )}
            </Field>
          )}
        />

        <Controller
          control={control}
          name="subject"
          render={({ field }) => (
            <Field data-invalid={!!errors.subject}>
              <FieldLabel htmlFor="meetingSubject">Subject</FieldLabel>

              <Input id="meetingSubject" {...field} />

              {errors.subject && <FieldError>{errors.subject.message}</FieldError>}
            </Field>
          )}
        />

        <Controller
          control={control}
          name="occurredOn"
          render={({ field }) => (
            <Field data-invalid={!!errors.occurredOn}>
              <FieldLabel htmlFor="meetingOccurredOn">Raised on</FieldLabel>

              <Input id="meetingOccurredOn" type="date" {...field} />

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
              <FieldLabel htmlFor="meetingScheduledFor">
                Scheduled for (optional)
              </FieldLabel>

              <Input id="meetingScheduledFor" type="date" {...field} />

              {errors.scheduledFor && (
                <FieldError>{errors.scheduledFor.message}</FieldError>
              )}
            </Field>
          )}
        />
      </FieldGroup>

      {mutation.isError && (
        <p className="text-sm text-destructive" data-testid="begin-meeting-error">
          {(mutation.error as Error).message}
        </p>
      )}

      <div className="flex justify-end">
        {/* "Record", not "Record meeting": the page's action carries the full
            name (docs/engineering/accessible-names.md). */}
        <Button type="submit" disabled={mutation.isPending}>
          {mutation.isPending ? "Recording..." : "Record"}
        </Button>
      </div>
    </form>
  );
}
